using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

public static class RSAHelper
{
   [MenuItem("Tools/生成RSA")]
   public static void GenerateRSA()
   {
       RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
        Debug.Log("公钥:"+provider.ToXmlString(false)); 
        Debug.Log("私钥:"+provider.ToXmlString(true)); 
   }
}
