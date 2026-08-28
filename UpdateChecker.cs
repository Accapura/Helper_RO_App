using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HelperGos
{
    /// <summary>
    /// Checks a public Яндекс.Диск folder for a newer HelperGos build and,
    /// with the user's permission, downloads and installs it in place.
    ///
    /// No version.txt, and no version number encoded in the filename either
    /// -- "newer" is decided purely by comparing the actual application
    /// itself: this build's own exe is hashed (SHA-256) and compared against
    /// the hash of whatever exe currently sits on Яндекс.Диск (that hash is
    /// ordinary file metadata, so it costs one cheap API call, not a
    /// download). If the bytes differ, there's a different build published
    /// -- no separate version label to keep in sync by hand anywhere.
    ///
    /// How to publish an update:
    ///   1. Build/publish the new HelperGos.exe as usual (dotnet publish ...).
    ///   2. Upload it to the SAME public Яндекс.Диск folder as <see cref="PublicFolderUrl"/>,
    ///      overwriting the previous HelperGos.exe there (same filename every
    ///      time -- the name plays no role in the check).
    /// That's it: every user's app checks this folder next time it starts and
    /// offers ("Доступно обновление ... Обновить сейчас?") to update itself.
    /// No server, no accounts, no API token, no text file -- it's all done
    /// through Яндекс.Диск's public (unauthenticated) resource API, off the
    /// exe's own content.
    /// </summary>
    public static class UpdateChecker
    {
        // Public "Поделиться" link to the Яндекс.Диск folder holding
        // HelperGos.exe. Change this if the folder ever moves.
        // Public (not private) so DownloadLinksDialog can reuse the exact
        // same URL for its "Яндекс.Диск" link instead of duplicating it.
        public const string PublicFolderUrl = "https://disk.yandex.ru/d/rcNRU6IoAKc-Hg";

        private const string ApiBase = "https://cloud-api.yandex.net/v1/disk/public/resources";

        // Fixed filename on Яндекс.Диск -- always overwritten in place with
        // each new build. Naming plays no part in detecting an update; only
        // the file's own content (its hash) does.
        private const string RemoteExeName = "/HelperGos.exe";

        public sealed class UpdateInfo
        {
            /// <summary>Human-friendly label for the dialog -- the remote
            /// build's last-modified date, since there's no version number
            /// to show ("Доступна сборка от 05.09.2026").</summary>
            public string RemoteLabel = "";
            public string DownloadUrl = "";
        }

        /// <summary>
        /// Compares the SHA-256 of this running exe against the SHA-256 of
        /// whatever exe currently sits in the Яндекс.Диск folder (a cheap
        /// metadata call -- the remote file itself is never downloaded just
        /// to check). Returns update info if they differ, or null if
        /// they're identical -- or on any failure (offline, API down,
        /// folder moved, can't hash the local exe, etc.): a background
        /// version check must never block or crash the app from starting.
        /// </summary>
        public static async Task<UpdateInfo?> CheckAsync()
        {
            try
            {
                string? currentExePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (currentExePath == null) return null;

                string localHash = await ComputeSha256Async(currentExePath);

                using var http = new HttpClient();
                http.Timeout = TimeSpan.FromSeconds(8);

                var (remoteHash, downloadUrl, modified) = await GetRemoteExeMetaAsync(http, RemoteExeName);
                if (remoteHash == null || downloadUrl == null) return null;

                if (string.Equals(remoteHash, localHash, StringComparison.OrdinalIgnoreCase)) return null;

                string label = modified.HasValue
                    ? $"сборка от {modified.Value:dd.MM.yyyy}"
                    : "новая сборка";

                return new UpdateInfo
                {
                    RemoteLabel = label,
                    DownloadUrl = downloadUrl,
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Downloads the new exe to a temp file (reporting 0-100 download
        /// progress via <paramref name="progress"/> as it goes, when the
        /// server reports a content length), then hands off to a tiny
        /// generated batch script that waits for this process to exit,
        /// overwrites the running exe with the new one, relaunches it, and
        /// deletes itself. Call only after the user has confirmed the update --
        /// this method ends the current process (Environment.Exit) on success.
        /// </summary>
        public static async Task InstallAndRestartAsync(string downloadUrl, IProgress<int>? progress = null)
        {
            string currentExePath = Process.GetCurrentProcess().MainModule?.FileName
                ?? throw new InvalidOperationException("Не удалось определить путь к текущему exe.");

            string tempDir = Path.Combine(Path.GetTempPath(), "HelperGosUpdate");
            Directory.CreateDirectory(tempDir);
            string newExePath = Path.Combine(tempDir, "HelperGos_new.exe");

            using (var http = new HttpClient())
            {
                http.Timeout = TimeSpan.FromMinutes(5);

                using var response = await http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                long? totalBytes = response.Content.Headers.ContentLength;
                await using var contentStream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = new FileStream(newExePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);

                var buffer = new byte[81920];
                long totalRead = 0;
                int read;
                while ((read = await contentStream.ReadAsync(buffer)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, read));
                    totalRead += read;
                    if (totalBytes is > 0)
                        progress?.Report((int)Math.Min(100, totalRead * 100 / totalBytes.Value));
                }
            }

            progress?.Report(100);

            int pid = Environment.ProcessId;
            string scriptPath = Path.Combine(tempDir, "apply_update.bat");
            string script = $"""
            @echo off
            :wait
            tasklist /fi "PID eq {pid}" | findstr /I "{pid}" >nul
            if not errorlevel 1 (
                timeout /t 1 /nobreak > nul
                goto wait
            )
            move /y "{newExePath}" "{currentExePath}" > nul
            start "" "{currentExePath}"
            del "%~f0"
            """;
            await File.WriteAllTextAsync(scriptPath, script, Encoding.ASCII);

            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"\"{scriptPath}\"\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
            };
            Process.Start(psi);

            Environment.Exit(0);
        }

        private static async Task<string> ComputeSha256Async(string filePath)
        {
            await using var stream = File.OpenRead(filePath);
            using var sha = SHA256.Create();
            byte[] hash = await sha.ComputeHashAsync(stream);
            return Convert.ToHexString(hash);
        }

        // "path" here is the item's path *inside* the shared public folder
        // (e.g. "/HelperGos.exe"), not a real filesystem path -- Яндекс.Диск's
        // public resources API accepts public_key + path together and returns
        // that one item's metadata (sha256, last-modified date, a temporary
        // direct-download link) without transferring the file's own content.
        private static async Task<(string? sha256, string? downloadUrl, DateTimeOffset? modified)> GetRemoteExeMetaAsync(HttpClient http, string relativePath)
        {
            string encodedPublicKey = Uri.EscapeDataString(PublicFolderUrl);
            string encodedPath = Uri.EscapeDataString(relativePath);
            string fields = Uri.EscapeDataString("file,sha256,modified");
            string metaUrl = $"{ApiBase}?public_key={encodedPublicKey}&path={encodedPath}&fields={fields}";

            using var resp = await http.GetAsync(metaUrl);
            if (!resp.IsSuccessStatusCode) return (null, null, null);

            string json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            string? sha256 = doc.RootElement.TryGetProperty("sha256", out var shaProp) ? shaProp.GetString() : null;
            string? downloadUrl = doc.RootElement.TryGetProperty("file", out var fileProp) ? fileProp.GetString() : null;
            DateTimeOffset? modified = null;
            if (doc.RootElement.TryGetProperty("modified", out var modProp)
                && DateTimeOffset.TryParse(modProp.GetString(), out var parsedModified))
                modified = parsedModified;

            return (sha256, downloadUrl, modified);
        }
    }
}
