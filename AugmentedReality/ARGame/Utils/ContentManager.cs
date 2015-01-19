using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Graphics;

namespace ARGame
{
    public class ContentManager
    {

        public Dictionary<string, SpriteFont> FontTable { get; private set; }
        public Dictionary<string, Model> ModelsTable { get; private set; }
        public Dictionary<string, Texture2D> TextureTable { get; private set; }

        public ContentManager()
        {
            FontTable = new Dictionary<string, SpriteFont>();
            ModelsTable = new Dictionary<string, Model>();
            TextureTable = new Dictionary<string, Texture2D>();
        }

        public void AddTexture(string key, Texture2D texture)
        {
            if (TextureTable.ContainsKey(key))
                TextureTable.Remove(key);
            TextureTable.Add(key, texture);
        }

        public Texture2D GetTexture(string key)
        {
            if (!TextureTable.ContainsKey(key))
                return null;
            return TextureTable[key];
        }
        public void AddModel(string key, Model model)
        {
            if (ModelsTable.ContainsKey(key))
                ModelsTable.Remove(key);
            ModelsTable.Add(key, model);
        }

        public Model GetModel(string key)
        {
            if (!ModelsTable.ContainsKey(key))
                return null;
            return ModelsTable[key];
        }

        public SpriteFont GetFont(string key)
        {
            if (!FontTable.ContainsKey(key))
                return null;
            return FontTable[key];
        }

        /*public static Model Arroy(float length, GraphicsDevice graphicDevice) 
        {
            Vector3 largo00 = new Vector3(-1, 0, 0);
            Vector3 largo01 = new Vector3(1, 0, 0);
            Vector3 largo10 = new Vector3(-1, length, 0);
            Vector3 largo11 = new Vector3(1, length, 0);
            Vector3 cabeza1 = new Vector3(-2, length, 0);
            Vector3 cabeza2 = new Vector3(0, length + 2, 0);
            Vector3 cabeza3 = new Vector3(2, length, 0);


            PrimitiveMesh pyramid = new PrimitiveMesh();
            VertexPositionColor[] verts = new VertexPositionColor[7];

            verts[0].Position = largo00;
            verts[1].Position = largo01;
            verts[2].Position = largo11;
            verts[3].Position = cabeza3;
            verts[4].Position = cabeza2;
            verts[5].Position = cabeza1;
            verts[6].Position = largo10;

            for(int i = 0; i < verts.Length; i++)
                verts[i].Color = Color.Red;

            pyramid.VertexBuffer = new VertexBuffer(graphicDevice,
                VertexPositionColor.VertexPositionColor * 7, BufferUsage.None);
            pyramid.SizeInBytes = VertexPositionNormalTexture.SizeInBytes;
            pyramid.VertexDeclaration = new VertexDeclaration(graphicDevice,
                VertexPositionColor.VertexElements);
            pyramid.VertexBuffer.SetData(verts);
            pyramid.NumberOfVertices = 7;

            pyramid.PrimitiveType = PrimitiveType.LineStrip;
            Model pyramidModel = new Model(pyramid);
            return pyramidModel;
            
        }*/
    }
}
