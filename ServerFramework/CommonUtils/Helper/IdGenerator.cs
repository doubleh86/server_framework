namespace ServerFramework.CommonUtils.Helper;

public static class IdGenerator
{
    private static long _sequence = 0;
    public static long NextId(long accountId)
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long seq = Interlocked.Increment(ref _sequence) % 4096; // 12비트
        long serverTag = accountId % 1024; // 10비트 (서버/유저 구분용 태그)

        // 시간(41) | 태그(10) | 시퀀스(12) = 총 63비트 (long 범위 안)
        return (timestamp << 22) | (serverTag << 12) | seq;
    }
}