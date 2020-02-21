using System.Threading;
using System.Threading.Tasks;
using Moq;
using NuixClient;
using NUnit.Framework;

namespace NuixClientTests
{
    public class MultiStreamReaderTest
    {

        [Test]
        public async Task TestReadingEmptyStream()
        {
            var reader1 = new Mock<IStreamReader>(MockBehavior.Strict);
            var multiStreamReader = new MultiStreamReader(new[] { reader1.Object });

            reader1.Setup(m => m.ReadLineAsync()).ReturnsAsync(null as string);

            var n1 = await multiStreamReader.ReadLineAsync();
            Assert.IsNull(n1);

            var n2 = await multiStreamReader.ReadLineAsync();
            Assert.IsNull(n2);

            reader1.VerifyAll();
        }

        [Test]
        public async Task TestReadingTextFromStream()
        {
            var reader1 = new Mock<IStreamReader>(MockBehavior.Strict);
            var multiStreamReader = new MultiStreamReader(new[] { reader1.Object });

            foreach (var str in new[]{String1, String2, String3})
            {
                reader1.Setup(m => m.ReadLineAsync()).ReturnsAsync(str);
                var a = await multiStreamReader.ReadLineAsync();
                Assert.AreEqual(str, a);
                reader1.VerifyAll();
            }

            reader1.Setup(m => m.ReadLineAsync()).ReturnsAsync(null as string);
            var n1 = await multiStreamReader.ReadLineAsync();
            Assert.IsNull(n1);

            var n2 = await multiStreamReader.ReadLineAsync();
            Assert.IsNull(n2);

            reader1.VerifyAll();
        }

        private const string String1 = "string 1";
        private const string String2 = "string 2";
        private const string String3 = "string 3";

        [Test]
        public async Task TestReadingTwoStreams()
        {
            var reader1 = new Mock<IStreamReader>(MockBehavior.Strict);
            var reader2 = new Mock<IStreamReader>(MockBehavior.Strict);

            var semaphore1 = new SemaphoreSlim(1);
            semaphore1.Wait();
            var semaphore2 = new SemaphoreSlim(1);
            semaphore2.Wait();

            reader1.Setup(m => m.ReadLineAsync())
                .Returns(()=> ReturnStringAfter(String1, semaphore1));

            reader2.Setup(m => m.ReadLineAsync())
                .Returns(() => ReturnStringAfter(String3, semaphore2));
            

            var multiStreamReader = new MultiStreamReader(new[] {reader1.Object, reader2.Object});

            semaphore1.Release();

            var r1 = await multiStreamReader.ReadLineAsync();
            Assert.AreEqual(String1, r1);

            reader1.VerifyAll();
            reader2.VerifyAll();

            reader1.Setup(m => m.ReadLineAsync())
                .ReturnsAsync(String2);

            var r2 = await multiStreamReader.ReadLineAsync();
            Assert.AreEqual(String2, r2);
            reader1.VerifyAll();
            reader2.VerifyAll();

            reader1.Setup(m => m.ReadLineAsync())
                .ReturnsAsync(null as string);

            semaphore2.Release();
                
            var r3 = await multiStreamReader.ReadLineAsync();

            Assert.AreEqual(String3, r3);
            reader1.VerifyAll();
            reader2.VerifyAll();

            reader2.Setup(m => m.ReadLineAsync())
                .ReturnsAsync(null as string);


            var n1 = await multiStreamReader.ReadLineAsync();
            Assert.IsNull(n1);

            var n2 = await multiStreamReader.ReadLineAsync();
            Assert.IsNull(n2);
        }


        private static async Task<string?> ReturnStringAfter(string? str, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();

            semaphore.Release();
            return str;
        }

       
    }
}
