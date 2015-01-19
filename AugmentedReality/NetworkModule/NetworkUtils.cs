using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

using System.Diagnostics;
using System.IO;



namespace NetworkModule
{
    public class NetworkUtils
    {
        /// <summary>
        /// Selecciona la red en base a los datos del adaptador
        /// </summary>
        /// <param name="ip">IP Address</param>
        /// <param name="subnetMask">Subnet Mask</param>
        /// <returns>Red</returns>
        public static IPAddress getNetwork(IPAddress ipLocal, IPAddress ipSubnetMask)
        {
            byte[] byIP = ipLocal.GetAddressBytes();
            byte[] byMask = ipSubnetMask.GetAddressBytes();

            int iLength = byIP.GetLength(0);
            byte[] bySubnet = new byte[iLength];

            for (int i = 0; i < iLength; i++)
            {
                bySubnet[i] = (byte)((~byMask[i]) | byIP[i]);
            }

            // Generates an Subnet Address from bytes.
            IPAddress ipSubnet = new IPAddress(bySubnet);

            // Converts Subnet to string.
            return ipSubnet;
        }


        /// <summary>
        /// Copia una parte de un arreglo de bytes en otro nuevo
        /// </summary>
        /// <param name="buffer">Arreglo original</param>
        /// <param name="offset">Inicio de copia</param>
        /// <param name="length">Fin de copia</param>
        /// <returns></returns>
        public static byte[] copyBuffer(byte[] buffer, int offset, int length)
        {
            byte[] newBuffer = new byte[length - offset];

            for (int i = 0; i < length - offset; i++)
                newBuffer[i] = buffer[i + offset];

            return newBuffer;
        }

        /// <summary>
        /// Copia parcialmente un arreglo de bytes en otro
        /// </summary>
        /// <param name="input">arreglo de input</param>
        /// <param name="output">arreglo de output</param>
        /// <param name="nInitInput">indice de inicio input</param>
        /// <param name="nInitOutput">indice de inicio output</param>
        /// <param name="nSize">tamaño a copiar</param>
        public static void copyPartialByteArray(byte[] input, byte[] output, int nInitInput, int nInitOutput, int nSize)
        {
            for (int i = nInitOutput, j = nInitInput; i < nInitOutput + nSize; i++, j++)
            {
                output[i] = input[j];
            }
        }

        public static int decodeInt16(byte[] data, int nInitInput, int nSize)
        {
            byte[] output = new byte[2];
            NetworkUtils.copyPartialByteArray(data, output, nInitInput, 0, nSize);
            int n = BitConverter.ToInt16(output, 0);
            return n;
        }

        //public static byte[] imageToByteArray(Image imageIn)
        //{            
        //    MemoryStream ms = new MemoryStream();
        //    imageIn.Save(ms, ImageFormat.Jpeg);
        //    return ms.ToArray();
        //}

        //public static Image byteArrayToImage(byte[] byteArrayIn)
        //{
        //    MemoryStream ms = new MemoryStream(byteArrayIn);
        //    Image returnImage = Image.FromStream(ms);
        //    return returnImage;
        //}

        /// <summary>
        /// Retorna el directorio actual
        /// </summary>
        /// <returns></returns>
        public static string getCurrentDirectory()
        {

            // Sets application path and name.
            string strAppFullFileName = System.Reflection.Assembly.
                GetExecutingAssembly().
                GetModules()[0].FullyQualifiedName;

            return System.IO.Path.GetDirectoryName(strAppFullFileName);

        }
    }            
}
