using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkModule;
using Microsoft.Xna.Framework;

namespace GameNetwork
{

    public class LevelInfo : INetworkObject
    {
        protected const int HEADER_SIZE = 2;
        protected int id;
        public int Id { get { return id; } }

        protected int groupId;
        public int GroupId { get { return groupId; } }

        public LevelInfo()
        {
            this.id = 0;
            this.groupId = 0;
        }

        public LevelInfo(int id, int groupId)
        {
            this.id = id;
            this.groupId = groupId;
        }
                
        #region INetworkObject Members

        public virtual byte[] Encode()
        {
            byte[] data = new byte[HEADER_SIZE];
            data[0] = (byte)id;
            data[1] = (byte)groupId;
            return data;
        }

        public virtual void Decode(byte[] data)
        {
            id = (int)data[0];
            groupId = (int)data[1];
        }

        #endregion
    }

    public class LevelState : LevelInfo
    {
        protected const int HEADER_SIZE = 3;

        private bool success;
        public bool Success { get { return success; } }

        public LevelState() : base()
        {
            this.success = false;
        }

        public LevelState(int id, int groupId, bool success):base(id, groupId)
        {
            this.success = success;
        }

        #region INetworkObject Members

        public override byte[] Encode()
        {
            byte[] data = new byte[HEADER_SIZE];
            data[0] = (byte)id;
            data[1] = (byte)groupId;
            data[2] = (byte)(success ? 1 : 0);
            return data;
        }

        public override void Decode(byte[] data)
        {
            id = (int)data[0];
            groupId = (int)data[1];
            success = data[2] == 1 ? true : false;
        }

        #endregion
    }


    public class PlayerInfo : INetworkObject
    {
        private const int HEADER_SIZE = 2;
        private int id;
        public int Id { get { return id; } }

        private int groupId;
        public int GroupId { get { return groupId; } }

        public PlayerInfo()
        {
            this.id = 0;
            this.groupId = 0;
        }

        public PlayerInfo(int id, int groupId)
        {
            this.id = id;
            this.groupId = groupId;
        }

             
        #region INetworkObject Members

        public byte[] Encode()
        {
            byte[] data = new byte[HEADER_SIZE];
            data[0] = (byte)id;
            data[1] = (byte)groupId;
            return data;
        }

        public void Decode(byte[] data)
        {
            id = (int)data[0];
            groupId = (int)data[1];
        }

        #endregion
    }

    public class ObjectState : ICloneable, INetworkObject
    {
        protected const int FLOAT_SIZE = 4;
        protected const int HEADER_SIZE = 3;            

        protected int id;
        public int Id { get { return id; } }

        protected int groupId;
        public int GroupId { get { return groupId; } }

        protected bool isAlive;
        public bool IsAlive { get { return isAlive; } }

        protected Vector3 position;
        public Vector3 Position { get { return position; } }


        public ObjectState()
        {
            id = 0;
            groupId = 0;
            isAlive = false;
            position = new Vector3(0, 0, 0);
        }

        public ObjectState(int id, int groupId, bool isAlive, Vector3 position)
        {
            this.id = id;
            this.groupId = groupId;
            this.isAlive = isAlive;
            this.position = new Vector3(position.X, position.Y, position.Z);
        }


        public override bool Equals(object obj)
        {
            if (!(obj is ObjectState))
                throw new ArgumentException("Object is not of ObjectState type");

            ObjectState state = obj as ObjectState;
            return this.position.Equals(state.Position);
        }

        public override string ToString()
        {
            return id.ToString();
        }

        #region ICloneable Members

        public virtual object Clone()
        {
            ObjectState obj = new ObjectState(id, groupId, isAlive, position);
            return obj;
        }

        #endregion

        #region INetworkObject Members

        public virtual byte[] Encode()
        {
            byte[] data = new byte[3 * FLOAT_SIZE + HEADER_SIZE];

            data[0] = (byte)id;
            data[1] = (byte)groupId;
            data[2] = isAlive ? (byte)1 : (byte)0;
            ByteHelper.FillByteArray(ref data, HEADER_SIZE, BitConverter.GetBytes(position.X));
            ByteHelper.FillByteArray(ref data, HEADER_SIZE + FLOAT_SIZE, BitConverter.GetBytes(position.Y));
            ByteHelper.FillByteArray(ref data, HEADER_SIZE + 2 * FLOAT_SIZE, BitConverter.GetBytes(position.Z));

            return data;
        }

        public virtual void Decode(byte[] data)
        {
            id = (int)data[0];
            groupId = (int)data[1];
            isAlive = data[2] == 1;
            position.X = ByteHelper.ConvertToFloat(data, HEADER_SIZE);
            position.Y = ByteHelper.ConvertToFloat(data, HEADER_SIZE + FLOAT_SIZE);
            position.Z = ByteHelper.ConvertToFloat(data, HEADER_SIZE + 2 * FLOAT_SIZE);
        }
        #endregion
    }

    public class ArrowObjectState : ObjectState
    {
        protected float length;
        public float Length { get { return length; } }
        protected float angle;
        public float Angle { get { return angle; } }

        public ArrowObjectState()
            : base()
        {
            this.length = 0;
            this.angle = 0;
        }

        public ArrowObjectState(int id, int groupId, bool isAlive, Vector3 position, float length, float angle)
            : base(id, groupId, isAlive, position)
        {            
            this.length = length;
            this.angle = angle;
        }

        public override byte[] Encode()
        {
            byte[] data = new byte[5 * FLOAT_SIZE + HEADER_SIZE];

            data[0] = (byte)id;
            data[1] = (byte)groupId;
            data[2] = isAlive ? (byte)1 : (byte)0;
            ByteHelper.FillByteArray(ref data, HEADER_SIZE, BitConverter.GetBytes(position.X));
            ByteHelper.FillByteArray(ref data, HEADER_SIZE + FLOAT_SIZE, BitConverter.GetBytes(position.Y));
            ByteHelper.FillByteArray(ref data, HEADER_SIZE + 2 * FLOAT_SIZE, BitConverter.GetBytes(position.Z));
            ByteHelper.FillByteArray(ref data, HEADER_SIZE + 3 * FLOAT_SIZE, BitConverter.GetBytes(length));
            ByteHelper.FillByteArray(ref data, HEADER_SIZE + 4 * FLOAT_SIZE, BitConverter.GetBytes(angle));

            return data;
        }

        public override void Decode(byte[] data)
        {
            id = (int)data[0];
            groupId = (int)data[1];
            isAlive = data[2] == 1;
            position.X = ByteHelper.ConvertToFloat(data, HEADER_SIZE);
            position.Y = ByteHelper.ConvertToFloat(data, HEADER_SIZE + FLOAT_SIZE);
            position.Z = ByteHelper.ConvertToFloat(data, HEADER_SIZE + 2 * FLOAT_SIZE);
            length = ByteHelper.ConvertToFloat(data, HEADER_SIZE + 3 * FLOAT_SIZE);
            angle = ByteHelper.ConvertToFloat(data, HEADER_SIZE + 4 * FLOAT_SIZE);
        }

    }

    public class MetaDataObjectState : ObjectState
    {
        protected float charge;
        public float Charge { get { return charge; } }
        
        public MetaDataObjectState()
            : base()
        {
            this.charge = 0;
        }

        public MetaDataObjectState(int id, int groupId, bool isAlive, Vector3 position, float charge)
            : base(id, groupId, isAlive, position)
        {
            this.charge = charge;
        }

        public override byte[] Encode()
        {
            byte[] data = new byte[4 * FLOAT_SIZE + HEADER_SIZE];

            data[0] = (byte)id;
            data[1] = (byte)groupId;
            data[2] = isAlive ? (byte)1 : (byte)0;
            ByteHelper.FillByteArray(ref data, HEADER_SIZE, BitConverter.GetBytes(position.X));
            ByteHelper.FillByteArray(ref data, HEADER_SIZE + FLOAT_SIZE, BitConverter.GetBytes(position.Y));
            ByteHelper.FillByteArray(ref data, HEADER_SIZE + 2 * FLOAT_SIZE, BitConverter.GetBytes(position.Z));
            ByteHelper.FillByteArray(ref data, HEADER_SIZE + 3 * FLOAT_SIZE, BitConverter.GetBytes(charge));

            return data;
        }

        public override void Decode(byte[] data)
        {
            id = (int)data[0];
            groupId = (int)data[1];
            isAlive = data[2] == 1;
            position.X = ByteHelper.ConvertToFloat(data, HEADER_SIZE);
            position.Y = ByteHelper.ConvertToFloat(data, HEADER_SIZE + FLOAT_SIZE);
            position.Z = ByteHelper.ConvertToFloat(data, HEADER_SIZE + 2 * FLOAT_SIZE);
            charge = ByteHelper.ConvertToFloat(data, HEADER_SIZE + 3 * FLOAT_SIZE);
        }

    }

    public class PlayerState : ObjectState
    {
        protected float charge;
        public float Charge { get { return charge; } }

        public PlayerState() 
            : base()
        {
            charge = 0;
        }

        public PlayerState(int id, int groupId, bool isAlive, Vector3 position, float charge)
            : base(id, groupId, isAlive, position)
        {            
            this.charge = charge;
        }

        public override byte[] Encode()
        {
            byte[] data = new byte[4 * FLOAT_SIZE + HEADER_SIZE];

            data[0] = (byte)id;
            data[1] = (byte)groupId;
            data[2] = isAlive ? (byte)1 : (byte)0;
            ByteHelper.FillByteArray(ref data, HEADER_SIZE, BitConverter.GetBytes(position.X));
            ByteHelper.FillByteArray(ref data, HEADER_SIZE + FLOAT_SIZE, BitConverter.GetBytes(position.Y));
            ByteHelper.FillByteArray(ref data, HEADER_SIZE + 2 * FLOAT_SIZE, BitConverter.GetBytes(position.Z));
            ByteHelper.FillByteArray(ref data, HEADER_SIZE + 3 * FLOAT_SIZE, BitConverter.GetBytes(charge));

            return data;
        }

        public override void Decode(byte[] data)
        {
            id = (int)data[0];
            groupId = (int)data[1];
            isAlive = data[2] == 1;
            position.X = ByteHelper.ConvertToFloat(data, HEADER_SIZE);
            position.Y = ByteHelper.ConvertToFloat(data, HEADER_SIZE + FLOAT_SIZE);
            position.Z = ByteHelper.ConvertToFloat(data, HEADER_SIZE + 2 * FLOAT_SIZE);
            charge = ByteHelper.ConvertToFloat(data, HEADER_SIZE + 3 * FLOAT_SIZE);
        }

    }

   
    
}
