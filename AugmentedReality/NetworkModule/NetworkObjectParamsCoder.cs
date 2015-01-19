using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkModule;
using System.Reflection;

namespace NetworkModule
{
    /// <summary>
    /// Clase encargada de codificación de parámetros a bytes
    /// </summary>
    public class NetworkObjectParamsCoder : IParamsCoder
    {
        private Dictionary<Type, byte> typeIdMap;
        private Dictionary<byte, Type> idTypeMap;
        private byte currentId = 0;

        private readonly int SHORT_SIZE = 2;

        public NetworkObjectParamsCoder()
        {
            typeIdMap = new Dictionary<Type, byte>();
            idTypeMap = new Dictionary<byte, Type>();            
        }

        /// <summary>
        /// Agrega un nuevo tipo al codificador
        /// </summary>
        public bool AddType(Type type)
        {
            typeIdMap.Add(type, currentId);
            idTypeMap.Add(currentId, type);
            if (currentId == 255)
                return false;
            currentId++;
            return true;
        }

        #region IParamsCoder Members

        /// <summary>
        /// Codifica parametros a un arreglo de bytes
        /// </summary>
        public byte[] encode(Params param)
        {
            byte[] data = new byte[0];
            foreach (INetworkObject obj in param.ParamList)
            {
                byte[] objData = obj.Encode();
                byte[] dataSize = BitConverter.GetBytes((short)objData.Length);
                byte[] dataType = new byte[] { typeIdMap[obj.GetType()] };
                dataSize = ByteHelper.ConcatenateBytes(dataSize, dataType);
                byte[] paramData = ByteHelper.ConcatenateBytes(dataSize, objData);
                data = ByteHelper.ConcatenateBytes(data, paramData);
            }
            return data;
        }

        /// <summary>
        /// Decodifica de una lista de bytes a parametros
        /// </summary>
        public Params decode(byte[] data)
        {
            Params p = new Params(ParamsType.NetworkObjectParams);
            int index = 0;

            while (index < data.Length)
            {
                int size = NetworkUtils.decodeInt16(data, index, SHORT_SIZE);
                Type type = idTypeMap[data[index + SHORT_SIZE]];
                ConstructorInfo ciObject = type.GetConstructor(new Type[] { });
                INetworkObject state = ciObject.Invoke(new Object[] { }) as INetworkObject;
                state.Decode(ByteHelper.Truncate(data, index + SHORT_SIZE + 1, size));
                index += SHORT_SIZE + 1 + size;
                p.addParam(state);                
            }

            return p;
        }

        #endregion
    }
}
