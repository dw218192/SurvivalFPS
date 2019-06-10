using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Saving
{
    public static class BinarySaver
    {
        private static readonly string m_SavePath = Application.persistentDataPath + "/SurvivalFPS/SavedGames/";
        private static readonly string m_Extension = ".sav";

        static BinarySaver()
        {
            bool exist = Directory.Exists(m_SavePath);

            if(!exist)
            {
                Directory.CreateDirectory(m_SavePath);
            }

            Debug.Log("BinarySaver: data path is " + m_SavePath);
        }

        public static void Save<T>(T data, string fileName) where T : class, ISerializable
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = File.Create(m_SavePath + fileName + m_Extension);
            binaryFormatter.Serialize(fileStream, data);
            fileStream.Close();
        }

        public static bool Load<T>(out T data, string fileName) where T : class, ISerializable
        {
            data = null;
            string file = m_SavePath + fileName + m_Extension;

            if(File.Exists(file))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();

                FileStream fileStream;
                try
                {
                    fileStream = File.Open(file, FileMode.Open);
                }
                catch(FileNotFoundException exception)
                {
                    Debug.LogWarningFormat("BinarySaver: file with name {0} is not found; exception message: {1}", fileName, exception.Message);
                    return false;
                }

                data = (T)binaryFormatter.Deserialize(fileStream);
                fileStream.Close();
                return true;
            }

            return false;
        }

        public static void Delete(string fileName)
        {
            File.Delete(m_SavePath + fileName + m_Extension);
        }
    }
}