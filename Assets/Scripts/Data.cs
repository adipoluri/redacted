using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace com.AstralSky.FPS
{
    public class Data : MonoBehaviour
    {
        public static void SaveProfile(ProfileData t_profile)
        {
            try 
            {
                string path = Application.persistentDataPath + "/profile.adi";

                if(File.Exists(path)) File.Delete(path);
            
                FileStream file = File.Create(path);

                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(file, t_profile);

                file.Close();
            }
            catch
            {
                Debug.Log("Something went wrong when saving the Profile File!");
            }
            
        }

        public static ProfileData LoadProfile()
        {
            ProfileData ret = new ProfileData();

            try
            {   
                 string path = Application.persistentDataPath + "/profile.adi";
                if(File.Exists(path))
                {
                    FileStream file = File.Open(path, FileMode.Open);
                    BinaryFormatter bf = new BinaryFormatter();
                    ret = (ProfileData) bf.Deserialize(file);

                }    
            }
            catch
            {
                Debug.Log("File Not Found!");
            }

            return ret;
        }
    }
}