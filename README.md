```csharp
    public static string SubstringByte(
        string? fieldValue,
        int startIndex,
        int maxByteCount,
        Encoding encoding)
    {
        if (string.IsNullOrEmpty(fieldValue)
            || startIndex < 0 || fieldValue.Length <= startIndex
            || maxByteCount <= 0)
        {
            return string.Empty;
        }

        int endIndex = Min(
                startIndex + maxByteCount,
                fieldValue.Length
            );
        SkipLowSurrogate(ref endIndex, fieldValue);

        int byteCount = encoding.GetByteCount(
                fieldValue,
                startIndex,
                endIndex - startIndex
            );

        if (byteCount <= maxByteCount)
        {
            return fieldValue[startIndex..endIndex];
        }

        int lowerIndex = startIndex;

        while (endIndex > lowerIndex)
        {
            int binaryIndex = endIndex - (endIndex - lowerIndex) / 2;
            SkipLowSurrogate(ref binaryIndex, fieldValue);

            byteCount = encoding.GetByteCount(
                    fieldValue,
                    startIndex,
                    binaryIndex - startIndex
                );

            if (byteCount > maxByteCount)
            {
                endIndex = binaryIndex != endIndex
                    ? binaryIndex
                    : lowerIndex;
            }
            else if (byteCount < maxByteCount)
            {
                lowerIndex = binaryIndex;
            }
            else
            {
                endIndex = lowerIndex = binaryIndex;
            }
        }

        return fieldValue[startIndex..endIndex];


        static void SkipLowSurrogate(ref int charIndex, string fieldValue)
        {
            if (charIndex < fieldValue.Length
                && char.IsLowSurrogate(fieldValue[charIndex]))
            {
                charIndex++;
            }
        }
    }
```

---
```csharp
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

        Assert.Equal(6, utf8.GetByteCount(input));

        string result;

        result = Fixer.SubstringByte(input, 0, 20, utf8);
        Assert.Equal(input, result);

        result = Fixer.SubstringByte(input, 0, 6, utf8);
        Assert.Equal(input, result);

        result = Fixer.SubstringByte(input, 0, 5, utf8);
        Assert.Equal("ab", result);
    }
}
```
