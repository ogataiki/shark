using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;

public class StringUtil
{
  const string salt = "xxsharkxxnogineg94yns";

  public static string GetHashMD5(string input)
  {
    var targetBytes = Encoding.UTF8.GetBytes(input + salt);

    // MD5ハッシュを計算
    var csp = new MD5CryptoServiceProvider();
    var hashBytes = csp.ComputeHash(targetBytes);

    // バイト配列を文字列に変換
    var hashStr = new StringBuilder();
    foreach (var hashByte in hashBytes)
    {
      hashStr.Append(hashByte.ToString("x2"));
    }
    return hashStr.ToString();
  }
}
