using Consilient.Infrastructure.Storage.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Consilient.Infrastructure.Storage.Tests
{
    [TestClass]
    public class LocalFileStorageTests
    {
        private string _testBasePath = null!;
        private ILogger<LocalFileStorage> _logger = null!;
        private LocalFileStorage _storage = null!;

        [TestInitialize]
        public void Setup()
        {
            _testBasePath = Path.Combine(Path.GetTempPath(), $"storage-tests-{Guid.NewGuid()}");
            _logger = NullLogger<LocalFileStorage>.Instance;

            var options = Options.Create(new FileStorageOptions
            {
                Provider = "Local",
                LocalPath = _testBasePath
            });

            _storage = new LocalFileStorage(options, _logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testBasePath))
            {
                Directory.Delete(_testBasePath, recursive: true);
            }
        }

        [TestMethod]
        public async Task SaveAsync_WithValidContent_SavesFileAndReturnsReference()
        {
            // Arrange
            var fileName = "test-file.txt";
            var content = "Hello, World!"u8.ToArray();
            using var stream = new MemoryStream(content);

            // Act
            var fileReference = await _storage.SaveAsync(fileName, stream);

            // Assert
            Assert.IsNotNull(fileReference);
            Assert.IsTrue(fileReference.Contains('/'));
            Assert.IsTrue(fileReference.EndsWith("test-file.txt"));
        }

        [TestMethod]
        public async Task SaveAsync_CreatesDirectoryAndFile()
        {
            // Arrange
            var fileName = "test-file.txt";
            var content = "Hello, World!"u8.ToArray();
            using var stream = new MemoryStream(content);

            // Act
            var fileReference = await _storage.SaveAsync(fileName, stream);

            // Assert
            var exists = await _storage.ExistsAsync(fileReference);
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public async Task GetAsync_WithExistingFile_ReturnsStream()
        {
            // Arrange
            var fileName = "test-file.txt";
            var expectedContent = "Hello, World!";
            using var saveStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(expectedContent));
            var fileReference = await _storage.SaveAsync(fileName, saveStream);

            // Act
            await using var retrievedStream = await _storage.GetAsync(fileReference);
            using var reader = new StreamReader(retrievedStream);
            var actualContent = await reader.ReadToEndAsync();

            // Assert
            Assert.AreEqual(expectedContent, actualContent);
        }

        [TestMethod]
        public async Task GetAsync_WithNonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var nonExistentReference = $"{Guid.NewGuid()}/non-existent.txt";

            // Act & Assert
            var exception = await Assert.ThrowsExactlyAsync<FileNotFoundException>(
                async () => await _storage.GetAsync(nonExistentReference));
            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public async Task ExistsAsync_WithExistingFile_ReturnsTrue()
        {
            // Arrange
            var fileName = "test-file.txt";
            using var stream = new MemoryStream("content"u8.ToArray());
            var fileReference = await _storage.SaveAsync(fileName, stream);

            // Act
            var exists = await _storage.ExistsAsync(fileReference);

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public async Task ExistsAsync_WithNonExistentFile_ReturnsFalse()
        {
            // Arrange
            var nonExistentReference = $"{Guid.NewGuid()}/non-existent.txt";

            // Act
            var exists = await _storage.ExistsAsync(nonExistentReference);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public async Task DeleteAsync_WithExistingFile_DeletesFile()
        {
            // Arrange
            var fileName = "test-file.txt";
            using var stream = new MemoryStream("content"u8.ToArray());
            var fileReference = await _storage.SaveAsync(fileName, stream);

            // Act
            await _storage.DeleteAsync(fileReference);

            // Assert
            var exists = await _storage.ExistsAsync(fileReference);
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public async Task DeleteAsync_WithNonExistentFile_DoesNotThrow()
        {
            // Arrange
            var nonExistentReference = $"{Guid.NewGuid()}/non-existent.txt";

            // Act & Assert (should not throw)
            await _storage.DeleteAsync(nonExistentReference);
        }

        [TestMethod]
        public async Task SaveAsync_SanitizesFileName()
        {
            // Arrange
            var unsafeFileName = "file<with>invalid:chars?.txt";
            using var stream = new MemoryStream("content"u8.ToArray());

            // Act
            var fileReference = await _storage.SaveAsync(unsafeFileName, stream);

            // Assert
            Assert.IsNotNull(fileReference);
            Assert.IsFalse(fileReference.Contains('<'));
            Assert.IsFalse(fileReference.Contains('>'));
            Assert.IsFalse(fileReference.Contains(':'));
            Assert.IsFalse(fileReference.Contains('?'));
        }

        [TestMethod]
        public async Task GetFullPath_NormalizesPathSeparators()
        {
            // Arrange - Save a file
            var fileName = "test-file.txt";
            using var stream = new MemoryStream("content"u8.ToArray());
            var fileReference = await _storage.SaveAsync(fileName, stream);

            // Act - Try to retrieve using forward slashes (portable format)
            var exists = await _storage.ExistsAsync(fileReference);

            // Assert
            Assert.IsTrue(exists);
            // The file reference uses forward slashes but should work on any OS
            Assert.IsTrue(fileReference.Contains('/'));
        }

        [TestMethod]
        public async Task SaveAsync_WithEmptyFileName_UsesDefaultFileName()
        {
            // Arrange
            var emptyFileName = "";
            using var stream = new MemoryStream("content"u8.ToArray());

            // Act
            var fileReference = await _storage.SaveAsync(emptyFileName, stream);

            // Assert
            Assert.IsNotNull(fileReference);
            Assert.IsTrue(fileReference.EndsWith("/file"));
        }

        [TestMethod]
        public void Constructor_WithNullLocalPath_UsesDefaultPath()
        {
            // Arrange
            var options = Options.Create(new FileStorageOptions
            {
                Provider = "Local",
                LocalPath = null
            });

            // Act
            var storage = new LocalFileStorage(options, _logger);

            // Assert - Should not throw and should use temp path
            Assert.IsNotNull(storage);
        }

        [TestMethod]
        public async Task DeleteAsync_CleansUpEmptyParentDirectory()
        {
            // Arrange
            var fileName = "test-file.txt";
            using var stream = new MemoryStream("content"u8.ToArray());
            var fileReference = await _storage.SaveAsync(fileName, stream);

            // Get the parent directory path
            var guidPart = fileReference.Split('/')[0];
            var parentDir = Path.Combine(_testBasePath, guidPart);

            // Act
            await _storage.DeleteAsync(fileReference);

            // Assert - Parent directory should be cleaned up
            Assert.IsFalse(Directory.Exists(parentDir));
        }

        [TestMethod]
        public async Task SaveAsync_WithLargeFile_SavesSuccessfully()
        {
            // Arrange
            var fileName = "large-file.bin";
            var largeContent = new byte[1024 * 1024]; // 1MB
            new Random().NextBytes(largeContent);
            using var stream = new MemoryStream(largeContent);

            // Act
            var fileReference = await _storage.SaveAsync(fileName, stream);

            // Assert
            var exists = await _storage.ExistsAsync(fileReference);
            Assert.IsTrue(exists);

            // Verify content
            await using var retrievedStream = await _storage.GetAsync(fileReference);
            using var memStream = new MemoryStream();
            await retrievedStream.CopyToAsync(memStream);
            CollectionAssert.AreEqual(largeContent, memStream.ToArray());
        }

        [TestMethod]
        public async Task SaveAsync_WithStreamAtNonZeroPosition_ResetsAndSavesCorrectly()
        {
            // Arrange - Simulate a stream that has been partially read
            var expectedContent = "Hello, World!";
            var content = System.Text.Encoding.UTF8.GetBytes(expectedContent);
            using var stream = new MemoryStream(content);

            // Advance stream position to simulate prior read
            stream.Position = 5;

            // Act
            var fileReference = await _storage.SaveAsync("test.txt", stream);

            // Assert - File should contain the full content, not just from position 5
            await using var retrievedStream = await _storage.GetAsync(fileReference);
            using var reader = new StreamReader(retrievedStream);
            var actualContent = await reader.ReadToEndAsync();

            Assert.AreEqual(expectedContent, actualContent);
        }

        [TestMethod]
        public async Task SaveAsync_WithStreamAtEnd_ResetsAndSavesCorrectly()
        {
            // Arrange - Simulate a stream that has been fully read
            var expectedContent = "Test content for stream position test";
            var content = System.Text.Encoding.UTF8.GetBytes(expectedContent);
            using var stream = new MemoryStream(content);

            // Read entire stream to move position to end
            _ = await new StreamReader(stream).ReadToEndAsync();
            Assert.AreEqual(stream.Length, stream.Position); // Verify we're at end

            // Act
            var fileReference = await _storage.SaveAsync("test.txt", stream);

            // Assert - File should contain the full content
            await using var retrievedStream = await _storage.GetAsync(fileReference);
            using var reader = new StreamReader(retrievedStream);
            var actualContent = await reader.ReadToEndAsync();

            Assert.AreEqual(expectedContent, actualContent);
        }
    }
}
