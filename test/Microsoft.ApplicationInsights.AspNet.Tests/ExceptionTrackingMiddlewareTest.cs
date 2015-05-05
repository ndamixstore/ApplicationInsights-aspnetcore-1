﻿namespace Microsoft.ApplicationInsights.AspNet
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.AspNet.Tests;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNet.Builder;
    using Xunit;
    using Microsoft.ApplicationInsights.Extensibility.Implementation;

    public class ExceptionTrackingMiddlewareTest
    {
        private ITelemetry sentTelemetry;

        [Fact]
        public async Task InvokeTracksExceptionThrownByNextMiddlewareAsHandledByPlatform()
        {
            RequestDelegate nextMiddleware = httpContext => { throw new Exception(); };
            var middleware = new ExceptionTrackingMiddleware(nextMiddleware, MockTelemetryClient());

            await Assert.ThrowsAnyAsync<Exception>(() => middleware.Invoke(null));

            Assert.Equal(ExceptionHandledAt.Platform, ((ExceptionTelemetry)sentTelemetry).HandledAt);
        }

        [Fact]
        public async Task SdkVersionIsPopulatedByMiddleware()
        {
            RequestDelegate nextMiddleware = httpContext => { throw new Exception(); };
            var middleware = new ExceptionTrackingMiddleware(nextMiddleware, MockTelemetryClient());

            await Assert.ThrowsAnyAsync<Exception>(() => middleware.Invoke(null));

            Assert.NotEmpty(sentTelemetry.Context.GetInternalContext().SdkVersion);
            Assert.Contains("aspnetv5", sentTelemetry.Context.GetInternalContext().SdkVersion);
        }

        private TelemetryClient MockTelemetryClient()
        {
            var telemetryChannel = new FakeTelemetryChannel { OnSend = telemetry => this.sentTelemetry = telemetry };

            var telemetryConfiguration = new TelemetryConfiguration();
            telemetryConfiguration.InstrumentationKey = "REQUIRED";
            telemetryConfiguration.TelemetryChannel = telemetryChannel;

            return new TelemetryClient(telemetryConfiguration);
        }
    }
}
