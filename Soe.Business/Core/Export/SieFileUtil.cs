using System;
using System.IO;
using System.Text;
using System.Linq;

public static class SieEncodingDetector
{
    public static Encoding DetectTextFileEncoding(MemoryStream fileStream)
    {
        // Save original position
        long originalPosition = fileStream.Position;
        
        try
        {
            // Reset to beginning of file
            fileStream.Position = 0;
            
            // Check for BOM (Byte Order Mark)
            var bom = new byte[4];
            int read = fileStream.Read(bom, 0, 4);
            
            // Check for UTF-8 BOM (EF BB BF)
            if (read >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
                return Encoding.UTF8;
                
            // Check for UTF-16 LE BOM (FF FE)
            if (read >= 2 && bom[0] == 0xFF && bom[1] == 0xFE)
                return Encoding.Unicode;
                
            // Check for UTF-16 BE BOM (FE FF)
            if (read >= 2 && bom[0] == 0xFE && bom[1] == 0xFF)
                return Encoding.BigEndianUnicode;
                
            // Check for UTF-32 LE BOM (FF FE 00 00)
            if (read >= 4 && bom[0] == 0xFF && bom[1] == 0xFE && bom[2] == 0x00 && bom[3] == 0x00)
                return Encoding.UTF32;
            
            // No BOM found, try to detect based on content
            fileStream.Position = 0;
            
            // Read a larger sample to analyze
            byte[] buffer = new byte[Math.Min(fileStream.Length, 4096)];
            fileStream.Read(buffer, 0, buffer.Length);
            
            // SIE-specific detection logic
            string content = Encoding.ASCII.GetString(buffer);
            
            // Check for Nordic characters (ÅÄÖåäö) in common encodings
            if (TryWithEncoding(buffer, Encoding.GetEncoding("windows-1252")) ||
                TryWithEncoding(buffer, Encoding.GetEncoding("ISO-8859-1")))
            {
                return Encoding.GetEncoding("windows-1252"); // ANSI (Windows-1252)
            }
            
            // Check for UTF-8 without BOM
            if (IsUtf8WithoutBom(buffer))
            {
                return Encoding.UTF8;
            }
            
            // For DOS encoding (CP437/CP850 in Nordic countries)
            if (ContainsNordicDosCharacters(buffer))
            {
                return Encoding.GetEncoding(850); // IBM850 (DOS Nordic)
            }
            
            // Default to ANSI (Windows-1252) if we can't determine
            return Encoding.GetEncoding("windows-1252");
        }
        finally
        {
            // Restore original position
            fileStream.Position = originalPosition;
        }
    }
    
    private static bool TryWithEncoding(byte[] buffer, Encoding encoding)
    {
        string content = encoding.GetString(buffer);
        
        // Look for SIE file markers
        if (content.Contains("#FLAGGA") || 
            content.Contains("#PROGRAM") || 
            content.Contains("#FORMAT") ||
            content.Contains("#SIETYP"))
        {
            // Check for Nordic characters
            return content.Contains("Å") || 
                   content.Contains("Ä") || 
                   content.Contains("Ö") ||
                   content.Contains("å") || 
                   content.Contains("ä") || 
                   content.Contains("ö");
        }
        
        return false;
    }
    
    private static bool IsUtf8WithoutBom(byte[] buffer)
    {
        // UTF-8 pattern checking
        int i = 0;
        while (i < buffer.Length)
        {
            // Check for multi-byte sequence
            if (buffer[i] >= 0x80)
            {
                // 2-byte sequence (110xxxxx 10xxxxxx)
                if ((buffer[i] & 0xE0) == 0xC0)
                {
                    if (i + 1 >= buffer.Length || (buffer[i + 1] & 0xC0) != 0x80)
                        return false;
                    i += 2;
                }
                // 3-byte sequence (1110xxxx 10xxxxxx 10xxxxxx)
                else if ((buffer[i] & 0xF0) == 0xE0)
                {
                    if (i + 2 >= buffer.Length || 
                        (buffer[i + 1] & 0xC0) != 0x80 ||
                        (buffer[i + 2] & 0xC0) != 0x80)
                        return false;
                    i += 3;
                }
                // 4-byte sequence (11110xxx 10xxxxxx 10xxxxxx 10xxxxxx)
                else if ((buffer[i] & 0xF8) == 0xF0)
                {
                    if (i + 3 >= buffer.Length || 
                        (buffer[i + 1] & 0xC0) != 0x80 ||
                        (buffer[i + 2] & 0xC0) != 0x80 ||
                        (buffer[i + 3] & 0xC0) != 0x80)
                        return false;
                    i += 4;
                }
                else
                    return false;
            }
            else
            {
                i++;
            }
        }
        return true;
    }
    
    private static bool ContainsNordicDosCharacters(byte[] buffer)
    {
        // DOS CP850 codes for Nordic characters
        byte[] nordicCharCodes = new byte[] 
        { 
            0x86, // Å
            0x8F, // Ä
            0x99, // Ö
            0x85, // å
            0x84, // ä
            0x94  // ö
        };
        
        return buffer.Any(b => nordicCharCodes.Contains(b));
    }
}
