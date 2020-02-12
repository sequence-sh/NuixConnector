using System.Threading.Tasks;
using Moq;
using NuixClient;
using NUnit.Framework;

namespace NuixClientTests
{
    public class MultiStreamReaderTest
    {

        [Test]
        public async Task TestMultiStreamReader()
        {

            var reader1 = new Mock<IStreamReader>(MockBehavior.Strict);
            var reader2 = new Mock<IStreamReader>(MockBehavior.Strict);


            const string string1 = "string 1";
            const string string2 = "string 2";
            const string string3 = "string 3";

            reader1.Setup(m => m.ReadLineAsync())
                .Returns(() => ReturnStringAfter(string1, 1));
            
            
            reader2.Setup(m => m.ReadLineAsync())
                .Returns(() => ReturnStringAfter(string3, 3000));

            var multiStreamReader = new MultiStreamReader(new[] {reader1.Object, reader2.Object});

            var r1 = await multiStreamReader.ReadLineAsync();
            Assert.AreEqual(string1, r1);
            reader1.Verify();
            reader2.Verify();

            
            reader1.Setup(m => m.ReadLineAsync())
                .Returns(() => ReturnStringAfter(string2, 1));

            var r2 = await multiStreamReader.ReadLineAsync();
            Assert.AreEqual(string2, r2);
            reader1.Verify();
            reader2.Verify();

            reader1.Setup(m => m.ReadLineAsync())
                .Returns(() => ReturnStringAfter(null, 1));

            var r3 = await multiStreamReader.ReadLineAsync();

            Assert.AreEqual(string3, r3);
            reader1.Verify();
            reader2.Verify();

            reader2.Setup(m => m.ReadLineAsync())
                .Returns(() => ReturnStringAfter(null, 1));


            var n1 = await multiStreamReader.ReadLineAsync();
            Assert.IsNull(n1);

            var n2 = await multiStreamReader.ReadLineAsync();
            Assert.IsNull(n2);
        }


        private async Task<string?> ReturnStringAfter(string? str, int milliseconds)
        {
            await Task.Delay(milliseconds);

            return str;
        }

       
    }
}
