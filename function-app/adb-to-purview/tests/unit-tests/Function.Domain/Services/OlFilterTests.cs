// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using Function.Domain.Services;
using Function.Domain.Helpers;
using Microsoft.Extensions.Logging;

namespace UnitTests.Function.Domain.Services
{
    public class OlFilterTests{
        private IEventParser _eventParser;

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
            var _log = _mockLoggerFactory.CreateLogger<OlFilterTests>();
            var _eventParser = new EventParser(_log);
            var parsedEvent = _eventParser.ParseOlEvent(msgEvent);
            var rslt = filterOlEvent.FilterOlMessage(parsedEvent);

            Xunit.Assert.Equal(expectedResult, rslt);
        }
    }
}