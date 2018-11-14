using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlHelper
{

        public class csIniFile
        {

            private static int iBufferLen = 256;

            [System.Runtime.InteropServices.DllImport("kernel32", EntryPoint = "WritePrivateProfileStringA", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi, SetLastError = true)]
            private static extern int WritePrivateProfileString(string pSection, string pKey, string pValue, string pFile);

            [System.Runtime.InteropServices.DllImport("kernel32", EntryPoint = "WritePrivateProfileStructA", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi, SetLastError = true)]
            private static extern int WritePrivateProfileStruct(string pSection, string pKey, string pValue, int pValueLen, string pFile);

            [System.Runtime.InteropServices.DllImport("kernel32", EntryPoint = "GetPrivateProfileStringA", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi, SetLastError = true)]
            private static extern int GetPrivateProfileString(string pSection, string pKey, string pDefault, byte[] prReturn, int pBufferLen, string pFile);

            [System.Runtime.InteropServices.DllImport("kernel32", EntryPoint = "GetPrivateProfileStructA", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi, SetLastError = true)]
            private static extern int GetPrivateProfileStruct(string pSection, string pKey, byte[] prReturn, int pBufferLen, string pFile);


            public static int BufferLen
            {

                get
                {
                    return iBufferLen;
                }

                set
                {

                    if (value > 32767)
                    {
                        iBufferLen = 32767;
                    }
                    else if (value < 1)
                    {
                        iBufferLen = 1;
                    }
                    else
                    {
                        iBufferLen = value;
                    }

                }

            }


            public static string GetSetting(string pSection, string pKey, string pDefault, string pFile)
            {
                return z_GetString(pSection, pKey, pDefault, pFile);
            }


            public static string GetSetting(string pSection, string pKey, string pFile)
            {
                return z_GetString(pSection, pKey, "", pFile);
            }


            public static void SaveSetting(string pSection, string pKey, string pValue, string pFile)
            {

                int i = WritePrivateProfileString(pSection, pKey, pValue, pFile);

            }


            public static void DeleteSetting(string pSection, string pKey, string pFile)
            {

                int i = WritePrivateProfileString(pSection, pKey, null, pFile);

            }

            public static void DeleteSection(string pSection, string pFile)
            {

                WritePrivateProfileString(pSection, null, null, pFile);

            }

            public static void GetValues(string pSection, ref Array pValues, string pFile)
            {
                pValues = z_GetString(pSection, null, null, pFile).Split(Convert.ToChar('\0'));
            }


            public static void GetSections(ref Array pSections, string pFile)
            {
                pSections = z_GetString(null, null, null, pFile).Split(Convert.ToChar('\0'));
            }

            private  static string z_GetString(string pSection, string pKey, string pDefault, string pFile)
            {

                string sRet = pDefault;
                byte[] bRet = new byte[(iBufferLen)];

                int i = GetPrivateProfileString(pSection, pKey, pDefault, bRet, iBufferLen, pFile);

                sRet = System.Text.Encoding.GetEncoding(0).GetString(bRet, 0, i).TrimEnd(Convert.ToChar('\0'));

                return sRet;

            }

        }

}
