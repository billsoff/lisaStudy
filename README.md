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
