namespace MysteryCodebase;

// --- Bought from a vendor in 2011. We can't change it. ---
public class TelePager3000
{
    public const int FlagUrgent = 0x01;
    public const int FlagUnicode = 0x08;

    public int Transmit(string msisdn, char[] payload, int flags)
    {
        var mode = (flags & FlagUnicode) != 0 ? "UCS2" : "GSM7";
        Console.WriteLine($"   📟 [{mode}] {msisdn}: {new string(payload)}");
        return payload.Length <= 160 ? 1 : (payload.Length + 152) / 153;
    }
}

// --- Our code ---
public interface IMessenger
{
    void Send(string phoneNumber, string text);
}

public class PagerLink(TelePager3000 pager) : IMessenger
{
    public void Send(string phoneNumber, string text)
    {
        var msisdn = phoneNumber.StartsWith('0') ? "46" + phoneNumber[1..] : phoneNumber.TrimStart('+');
        var flags = text.Any(c => c > 127) ? TelePager3000.FlagUnicode : 0;
        pager.Transmit(msisdn, text.ToCharArray(), flags);
    }
}
