using System.Text.RegularExpressions;
using System.Linq;

namespace Api.Utils;

public static class PhoneValidator
{
    /// <summary>
    /// Türkiye telefon numarasını doğrular ve normalize eder.
    /// Kabul edilen formatlar:
    /// - 05XX XXX XX XX (mobil, 11 haneli - 0 dahil)
    /// - 0XXX XXX XX XX (sabit hat, 11 haneli - 0 dahil)
    /// - +90 5XX XXX XX XX (uluslararası format)
    /// </summary>
    /// <param name="phone">Telefon numarası</param>
    /// <param name="normalized">Normalize edilmiş telefon numarası (0 ile başlayan 11 haneli)</param>
    /// <returns>Geçerli ise true</returns>
    public static bool IsValidTurkishPhone(string? phone, out string? normalized)
    {
        normalized = null;
        
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        // Boşluk, tire ve parantezleri kaldır
        var cleaned = Regex.Replace(phone.Trim(), @"[\s\-\(\)]", "");

        // +90 ile başlıyorsa kaldır ve 0 ekle
        if (cleaned.StartsWith("+90"))
        {
            cleaned = "0" + cleaned.Substring(3);
        }
        // 90 ile başlıyorsa (başında + yoksa) 0 ekle
        else if (cleaned.StartsWith("90") && cleaned.Length == 12)
        {
            cleaned = "0" + cleaned.Substring(2);
        }

        // 0 ile başlamalı ve 11 haneli olmalı (0 dahil)
        if (!cleaned.StartsWith("0") || cleaned.Length != 11)
            return false;

        // Sadece rakam olmalı
        if (!Regex.IsMatch(cleaned, @"^[0-9]+$"))
            return false;

        // Mobil numaralar: 05XX ile başlamalı (0 + 5XX + 7 hane = 11 hane)
        var prefix = cleaned.Substring(0, 3);
        
        // Mobil numaralar (05XX)
        if (prefix == "050" || prefix == "051" || prefix == "052" || prefix == "053" || 
            prefix == "054" || prefix == "055" || prefix == "056" || prefix == "057" || 
            prefix == "058" || prefix == "059")
        {
            // 05XX + 7 hane = 11 hane toplam
            if (Regex.IsMatch(cleaned, @"^05[0-9]{9}$"))
            {
                normalized = cleaned;
                return true;
            }
        }

        // Sabit hatlar için genel kontrol (0 ile başlayan 11 haneli)
        // 0 + alan kodu (3-4 hane) + 7-6 hane = 11 hane
        if (Regex.IsMatch(cleaned, @"^0[1-9]\d{9}$"))
        {
            normalized = cleaned;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Telefon numarasını normalize eder (0 ile başlayan 11 haneli format)
    /// </summary>
    public static string? NormalizePhone(string? phone)
    {
        if (IsValidTurkishPhone(phone, out var normalized))
            return normalized;
        return null;
    }
}

