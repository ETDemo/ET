using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class RSAHelper
{
   [MenuItem("Tools/生成RSA")]
   public static void GenerateRSA()
   {
        var rsa = RSA.Create();
        Debug.Log("公钥:"+rsa.ToXmlString(false)); 
        Debug.Log("私钥:"+rsa.ToXmlString(true));
   }
}
