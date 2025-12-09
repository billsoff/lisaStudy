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
