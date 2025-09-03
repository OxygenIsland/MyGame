// using System;
// using System.Security.Cryptography;
// using System.Text;

// namespace StarWorld.Common.Utility
// {
//     public class EncryptDecipherUtil
//     {
//         private static string key = "LOMESnZs7FUuEdR8YRLY18MyuX/K/bTy";

//         public static string Encrypt(string content)
//         {
//             return Encrypt(content, key);
//         }

//         //AES加密

//         public static string Encrypt(string content, string k)
//         {
//             byte[] keyBytes = UTF8Encoding.UTF8.GetBytes(k);

//             RijndaelManaged rm = new RijndaelManaged();

//             rm.Key = keyBytes;

//             rm.Mode = CipherMode.ECB;

//             rm.Padding = PaddingMode.PKCS7;

//             ICryptoTransform ict = rm.CreateEncryptor();

//             byte[] contentBytes = UTF8Encoding.UTF8.GetBytes(content);

//             byte[] resultBytes = ict.TransformFinalBlock(contentBytes, 0, contentBytes.Length);

//             return Convert.ToBase64String(resultBytes, 0, resultBytes.Length);

//         }

//         public static string Decipher(string content)
//         {
//             return Decipher(content, key);
//         }

//         //AES解密
//         public static string Decipher(string content, string k)
//         {

//             byte[] keyBytes = UTF8Encoding.UTF8.GetBytes(k);

//             RijndaelManaged rm = new RijndaelManaged();

//             rm.Key = keyBytes;

//             rm.Mode = CipherMode.ECB;

//             rm.Padding = PaddingMode.PKCS7;

//             ICryptoTransform ict = rm.CreateDecryptor();

//             byte[] contentBytes = Convert.FromBase64String(content);
//             //byte[] contentBytes = Base64UrlDecode(content);

//             byte[] resultBytes = ict.TransformFinalBlock(contentBytes, 0, contentBytes.Length);

//             return UTF8Encoding.UTF8.GetString(resultBytes);
//         }

//         public static byte[] Base64UrlDecode(string base64)
//         {
//             string padded = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
//             return Convert.FromBase64String(padded);
//         }

//         public static bool IsBase64Formatted(string input)
//         {
//             try
//             {
//                 Convert.FromBase64String(input);
//                 return true;
//             }
//             catch 
//             { 
//                 return false; 
//             }
//         }
//     }
// }
