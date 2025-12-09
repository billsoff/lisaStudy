```csharp
    public static string SubstringByte(
        string fieldValue,
        int startIndex,
        int fixedLength,
        Encoding encoding)
    {
        if (string.IsNullOrEmpty(fieldValue)
            || startIndex < 0 || fieldValue.Length <= startIndex
            || fixedLength <= 0)
        {
            return string.Empty;
        }

        int endIndex = Min(fixedLength + startIndex, fieldValue.Length);

        int charCount = endIndex - startIndex;
        int byteCount = encoding.GetByteCount(fieldValue, startIndex, charCount);

        // シングルバイトエンコーディングの場合
        if (charCount == byteCount)
        {
            return fieldValue[startIndex..endIndex];
        }

        int lowerIndex = startIndex;

        while (endIndex > lowerIndex)
        {
            int currentIndex = (endIndex - lowerIndex) / 2 + lowerIndex;

            if (char.IsLowSurrogate(fieldValue[currentIndex]))
            {
                currentIndex += 1;
            }

            if (currentIndex == lowerIndex)
            {
                currentIndex = endIndex;
            }

            charCount = currentIndex - startIndex;
            byteCount = encoding.GetByteCount(fieldValue, startIndex, charCount);

            if (byteCount == fixedLength)
            {
                endIndex = currentIndex;

                break;
            }

            if (byteCount > fixedLength && currentIndex == endIndex)
            {
                endIndex = lowerIndex;

                break;
            }

            if (byteCount >= fixedLength)
            {
                endIndex = currentIndex;
            }
            else
            {
                lowerIndex = currentIndex;
            }
        }

        return fieldValue[startIndex..endIndex];
    }

    public static string SubstringByte3(
        string fieldValue,
        int startIndex,
        int fixedLength,
        Encoding encoding)
    {
        if (string.IsNullOrEmpty(fieldValue)
            || startIndex < 0 || fieldValue.Length <= startIndex
            || fixedLength <= 0)
        {
            return string.Empty;
        }

        int endIndex;

        // シングルバイトエンコーディングの場合
        if (fieldValue.Length == encoding.GetByteCount(fieldValue))
        {
            endIndex = Min(startIndex + fixedLength, fieldValue.Length);

            return fieldValue[startIndex..endIndex];
        }

        int totalByteCount = 0,
            byteCount;

        Encoder encoder = encoding.GetEncoder();

        ReadOnlySpan<char> chars = fieldValue;
        Span<byte> buffer = new byte[encoding.GetMaxByteCount(1)];

        endIndex = startIndex;

        for (int index = startIndex; index < chars.Length; index++)
        {
            byteCount = encoder.GetBytes(
                chars.Slice(index, 1),
                bytes: buffer,
                flush: false);

            if (byteCount == 0)
            {
                continue;
            }

            if (totalByteCount + byteCount > fixedLength)
            {
                break;
            }

            totalByteCount += byteCount;
            endIndex = index + 1;
        }

        return fieldValue[startIndex..endIndex];
    }

```
---
```csharp
using System.Text;

using Truncate;

Console.OutputEncoding = Encoding.UTF8;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
Encoding sjis = Encoding.GetEncoding("shift_jis");

string input = "NME-00451G_武藤静香・関根雅和";
string result = Fixer.Truncate(input, sjis, 20);

Console.WriteLine($"{input}\r\n->\r\n{result} ({sjis.GetByteCount(result)})");

Console.WriteLine();
PrintCodes();

Console.WriteLine(sjis.GetString([129]));
Console.WriteLine(sjis.GetString([129, 69]));
Console.WriteLine(sjis.GetString([141]));
Console.WriteLine(sjis.GetString([141, 129]));

Encoder encoder = Encoding.UTF8.GetEncoder();
ReadOnlySpan<char> chars = "😀";

for (int i = 0; i < chars.Length; ++i)
{
    int n = encoder.GetByteCount(chars.Slice(i, 1), flush: false);
    
    if (n != 0)
    {
        encoder.Reset();
    }

    Console.WriteLine($"step {i}: returned {n}");
}

int total = 0;

// 先喂高代理
total += encoder.GetByteCount(chars.Slice(0, 1), flush: false);

// 再喂低代理，并告诉它“这是末尾”
total += encoder.GetByteCount(chars.Slice(1, 1), flush: true); // 这里清空

Console.WriteLine(total);   // 输出 4

Encoder enc = Encoding.UTF8.GetEncoder();

byte[] buf = new byte[20];
int b1 = enc.GetByteCount(new char[] { '\uD83D' }, 0, 1, flush: false); // 0
int b2 = enc.GetByteCount(new char[] { '\uDE00' }, 0, 1, flush: false); // 4
Console.WriteLine(b1 + b2);   // 4

ReadOnlySpan<char> hi = "\uD83D";   // 高代理
ReadOnlySpan<char> lo = "\uDE00";   // 低代理

b1 = enc.GetByteCount(hi, flush: false);
b2 = enc.GetByteCount(lo, flush: false);

Console.WriteLine($"b1 = {b1}, b2 = {b2}, total = {b1 + b2}");

void PrintCodes()
{
    byte[] codes;
    int index = 0;

    foreach (char ch in input)
    {
        codes = sjis.GetBytes(ch.ToString());

        Console.WriteLine($"{ch}\t{index}\t{string.Join(" ", codes)}");
        index += codes.Length;
    }
}
```
---
```charp
using System.Text;

using Truncate;

namespace UnitTest;

public class SubstringByteTest
{
    private readonly Encoding _sjis;

    public SubstringByteTest()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _sjis = Encoding.GetEncoding("shift_jis");
    }


    [Fact]
    public void Odd()
    {
        Assert.Empty(Fixer.SubstringByte(null, 0, 20, _sjis));
        Assert.Empty(Fixer.SubstringByte("", 0, 20, _sjis));

        Assert.Empty(Fixer.SubstringByte("ab", -1, 20, _sjis));
        Assert.Empty(Fixer.SubstringByte("ab", 2, 20, _sjis));

        Assert.Empty(Fixer.SubstringByte("ab", 0, 0, _sjis));
        Assert.Empty(Fixer.SubstringByte("ab", 0, -3, _sjis));
    }

    [Fact]
    public void SingleByteCodes()
    {
        string input = "0123456789012";
        string result = Fixer.SubstringByte(input, 0, 8, _sjis);

        Assert.Equal(8, _sjis.GetByteCount(result));
        Assert.Equal("01234567", result);
    }

    [Fact]
    public void SingleByteCodes2()
    {
        string input = "0123456789012";
        string result = Fixer.SubstringByte(input, 0, 13, _sjis);

        Assert.Equal(13, _sjis.GetByteCount(result));
        Assert.Equal(input, result);
    }

    [Fact]
    public void SingleByteCodes3()
    {
        string input = "0123456789012";
        string result = Fixer.SubstringByte(input, 0, 20, _sjis);

        Assert.Equal(13, _sjis.GetByteCount(result));
        Assert.Equal(input, result);
    }

    [Fact]
    public void Normal()
    {
        string input = "NME-00451G_武藤静香・関根雅和";
        string result = Fixer.SubstringByte(input, 0, 20, _sjis);

        Assert.Equal(19, _sjis.GetByteCount(result));
        Assert.Equal("NME-00451G_武藤静香", result);
    }

    [Fact]
    public void Normal2()
    {
        string input = "NME-00451G_武藤静香関・根雅和";
        string result = Fixer.SubstringByte(input, 0, 20, _sjis);

        Assert.Equal(19, _sjis.GetByteCount(result));
        Assert.Equal("NME-00451G_武藤静香", result);
    }

    [Fact]
    public void Normal3()
    {
        string input = "NME-00451G_武藤静香a・関根雅和";
        string result = Fixer.SubstringByte(input, 0, 20, _sjis);

        Assert.Equal(20, _sjis.GetByteCount(result));
        Assert.Equal("NME-00451G_武藤静香a", result);
    }

    [Fact]
    public void Normal4()
    {
        string input = "NME-00451G_武藤静香・関根雅和";
        string result = Fixer.SubstringByte(input, 1, 20, _sjis);

        Assert.Equal(20, _sjis.GetByteCount(result));
        Assert.Equal("ME-00451G_武藤静香・", result);
    }

    [Fact]
    public void Normal5()
    {
        string input = "NME-00451G_武藤静香・関根雅和";
        string result = Fixer.SubstringByte(input, 0, 29, _sjis);

        Assert.Equal(29, _sjis.GetByteCount(result));
        Assert.Equal(input, result);
    }

    [Fact]
    public void Normal6()
    {
        string input = "NME-00451G_武藤静香・関根雅和";
        string result = Fixer.SubstringByte(input, 0, 30, _sjis);

        Assert.Equal(29, _sjis.GetByteCount(result));
        Assert.Equal(input, result);
    }

    [Fact]
    public void SurrogatePair()
    {
        Encoding utf8 = Encoding.UTF8;
        string input = "ab😀"; // 0xD83D 0xDE00 -> 0x1F600 十进制 = 128512） UTF-8: 4 bytes

        string result;

        result = Fixer.SubstringByte(input, 0, 6, utf8);
        Assert.Equal(input, result);

        result = Fixer.SubstringByte(input, 0, 5, utf8);
        Assert.Equal("ab", result);
    }
}
```
