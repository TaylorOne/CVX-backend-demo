using CVX.Models;
using Serilog;

namespace CVX.Services
{
    public interface ITokenReleaseService
    {
        public bool ReleaseTokens(Collaborative collab);
        public decimal CalculateTokensReleasedPerGivenCycleNumber(Collaborative collab, int cycleNum);
    }

    public class TokenReleaseService : ITokenReleaseService
    {
        public decimal CalculateTokensToRelease(Collaborative collab)
        {
            if (collab.LaunchTokensCreated == null || collab.TokenReleaseRate == null || collab.LaunchCyclePeriod == null || collab.SecondTokenReleaseDate == null)
                return 0;

            var now = DateTime.UtcNow;
            if (now < collab.SecondTokenReleaseDate)
                return 0;

            int totalCyclesPassed = 1;
            int lastCycle = 0; ;

            // initial condition: collab has been approved with second token release upon approval
            // because tokens are released immediately to the collaborative upon creation (it's first cycle)
            // all calculations below assume that when the collab is approved and the SecondTokenReleaseDate that starts cycle #2
            if (now >= collab.SecondTokenReleaseDate && collab.SecondTokenReleaseDate > collab.LastTokenRelease)
            {
                totalCyclesPassed = 2;
                lastCycle = 1;
            }
            else
            {
                totalCyclesPassed = (int)((now - collab.SecondTokenReleaseDate.Value).TotalDays / (collab.LaunchCyclePeriod.Value * 7));
                if (totalCyclesPassed < 1)
                    totalCyclesPassed = 2;

                if (collab.LastTokenRelease != null)
                {
                    lastCycle = (int)((collab.LastTokenRelease.Value - collab.SecondTokenReleaseDate.Value).TotalDays / (collab.LaunchCyclePeriod.Value * 7)) + 1;
                }
            }

            int missedCycles = totalCyclesPassed - lastCycle;
            if (missedCycles < 1)
                return 0;

            decimal tokensToRelease = 0;
            for (int i = lastCycle + 1; i <= totalCyclesPassed; i++)
            {
                tokensToRelease += collab.LaunchTokensCreated.Value * collab.TokenReleaseRate.Value *
                    (decimal)Math.Pow((double)(1 - collab.TokenReleaseRate.Value), i - 1);
            }

            return tokensToRelease;
        }

        public bool ReleaseTokens(Collaborative collab)
        {
            if (IsReleaseDue(collab, DateTime.UtcNow))
            {
                decimal toRelease = CalculateTokensToRelease(collab);

                Log.Information($"Releasing {toRelease} tokens for Collab {collab.Id}");

                if (toRelease > 0)
                {
                    collab.LaunchTokensBalance += toRelease;
                    collab.LastTokenRelease = DateTime.UtcNow;
                    return true;

                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool IsReleaseDue(Collaborative collab, DateTime now)
        { 
            if (collab.SecondTokenReleaseDate == null || collab.LaunchCyclePeriod == null)
                return false;

            Log.Information($"Checking token release for Collab {collab.Id}, DateTime.Now: {now}, LastTokenRelease: {collab.LastTokenRelease}, SecondTokenReleaseDate: {collab.SecondTokenReleaseDate}, LaunchCyclePeriod: {collab.LaunchCyclePeriod}");
            Log.Information($"Checking token release for Collab {collab.Id}, DateTime.Now: {now}, LastTokenRelease: {collab.LastTokenRelease}, SecondTokenReleaseDate: {collab.SecondTokenReleaseDate}, LaunchCyclePeriod: {collab.LaunchCyclePeriod}");

            // handle initial condition where SecondTokenReleaseDate has been triggered
            if (now >= collab.SecondTokenReleaseDate && collab.LastTokenRelease < collab.SecondTokenReleaseDate)
                return true;

            return (now - collab.LastTokenRelease.Value).TotalDays >= collab.LaunchCyclePeriod * 7;
        }

        public decimal CalculateTokensReleasedPerGivenCycleNumber(Collaborative collab, int cycleNum)
        {
            if (collab == null || cycleNum < 1)
                return 0;

            return collab.LaunchTokensCreated.Value * collab.TokenReleaseRate.Value *
                    (decimal)Math.Pow((double)(1 - collab.TokenReleaseRate.Value), cycleNum - 1);
        }
    }
}
