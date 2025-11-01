using CVX.Models;
using CVX.Services;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Engine.ClientProtocol;
using Xunit.Abstractions;

namespace CVX.Tests
{
    public class TokenReleaseServiceTests
    {
        private readonly ITestOutputHelper output;

        public TokenReleaseServiceTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void CalculateTokensToRelease_ReturnsZero_IfBeforeInitialRelease()
        {
            var collab = new Collaborative
            {
                LaunchTokensCreated = 10000,
                TokenReleaseRate = 0.10m,
                LaunchCyclePeriod = 12,
                SecondTokenReleaseDate = DateTime.UtcNow.AddDays(1),
                LaunchTokensBalance = 0
            };
            var service = new TokenReleaseService();
            Assert.Equal(0, service.CalculateTokensToRelease(collab));
        }

        [Fact]
        public void CalculateTokensToRelease_ReturnsZero_IfTokenInitialReleaseDateIsNull()
        {
            var collab = new Collaborative
            {
                LaunchTokensCreated = 10000,
                TokenReleaseRate = 0.10m,
                LaunchCyclePeriod = 12,
                SecondTokenReleaseDate = null,
                LaunchTokensBalance = 0
            };
            var service = new TokenReleaseService();
            Assert.Equal(0, service.CalculateTokensToRelease(collab));
        }

        [Fact]
        public void ReleaseTokens_UpdatesBalance_And_LastTokenRelease()
        {
            var collab = new Collaborative
            {
                LaunchTokensCreated = 10000,
                TokenReleaseRate = 0.10m,
                LaunchCyclePeriod = 12,
                SecondTokenReleaseDate = DateTime.UtcNow.AddDays(-90),
                LaunchTokensBalance = 0,
                LastTokenRelease = null
            };

            var service = new TokenReleaseService();
            var balanceBefore = collab.LaunchTokensBalance;
            service.ReleaseTokens(collab);
            var balanceAfter = collab.LaunchTokensBalance;
            var tokensReleased = balanceAfter - balanceBefore;

            output.WriteLine($"Tokens released: {tokensReleased}, New balance: {collab.LaunchTokensBalance}, Last release: {collab.LastTokenRelease}");

            Assert.True(collab.LaunchTokensBalance > 0);
            Assert.NotNull(collab.LastTokenRelease);
        }

        [Fact]
        public void CalculateTokensToRelease_ReturnsCorrectValue_AfterMultipleCycles()
        {
            var collab = new Collaborative
            {
                LaunchTokensCreated = 10000,
                TokenReleaseRate = 0.10m,
                LaunchCyclePeriod = 12,
                SecondTokenReleaseDate = DateTime.UtcNow.AddDays(-270),
                LaunchTokensBalance = 0,
                LastTokenRelease = null,
            };

            // Given that three cycles have passed and no tokens have been released yet,
            // this should release 2710 given the figures above
            var service = new TokenReleaseService();
            var tokensToRelease = service.CalculateTokensToRelease(collab);
            output.WriteLine($"Tokens to release: {tokensToRelease}");
            Assert.True(tokensToRelease == 2710);
        }

        [Fact]
        public void CalculateTokensReleasedPerGivenCycleNumber_ReturnsCorrectValue_AtOneCycle()
        {
            var collab = new Collaborative
            {
                LaunchTokensCreated = 10000,
                TokenReleaseRate = 0.10m,
            };

            var service = new TokenReleaseService();
            var tokensReleasedCycleOne = service.CalculateTokensReleasedPerGivenCycleNumber(collab, 1);

            Assert.True(tokensReleasedCycleOne == 1000);
        }

        [Fact]
        public void CalculateTokensReleasedPerGivenCycleNumber_ReturnsCorrectValue_AtTwoCycles()
        {
            var collab = new Collaborative
            {
                LaunchTokensCreated = 10000,
                TokenReleaseRate = 0.10m,
            };

            var service = new TokenReleaseService();
            var tokensReleasedCycleOne = service.CalculateTokensReleasedPerGivenCycleNumber(collab, 2);

            Assert.True(tokensReleasedCycleOne == 900);
        }
    }
}