using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using Function.Domain.Services;

namespace UnitTests.Function.Domain.Services
{
    public class OlFilterTests{

        private NullLoggerFactory _mockLoggerFactory;

        public OlFilterTests()
        {
           _mockLoggerFactory = new NullLoggerFactory();
        }

        [Theory]
        [ClassData(typeof(FilterOlEventTestData))]
        public void FilterOlEvent_bool_FilterGoodEvents(string msgEvent, bool expectedResult)
        {
            IOlFilter filterOlEvent = new OlFilter(_mockLoggerFactory);
            var rslt = filterOlEvent.FilterOlMessage(msgEvent);

            Xunit.Assert.Equal(expectedResult, rslt);
        }
    }
}