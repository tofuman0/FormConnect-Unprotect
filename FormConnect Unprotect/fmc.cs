using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;

namespace FormConnect_Unprotect
{
    internal class fmc
    {
        private byte[] fmcData;
        public bool Loaded = false;

        private Dictionary<string, Int32> pElements = new Dictionary<string, int>();

        public void LoadFMC(string FilePath)
        {
            using (FileStream fs = File.OpenRead(FilePath))
            {
                fmcData = new byte[fs.Length];
                if (fs.Read(fmcData, 0, (Int32)fs.Length) != fs.Length)
                    throw new Exception("Unable to read FMC file into buffer.");

                ProcessHeader();
            }
        }
        public void SaveFMC(string FilePath)
        {
            if (fmcData == null)
            {
                throw new Exception("FMC buffer is empty. Nothing to save.");
            }
            else
            {
                using (FileStream fs = File.OpenWrite(FilePath))
                {
                    fs.Write(fmcData);
                    return;
                }
            }
        }
        public byte[] GetFMCData()
        {
            if (fmcData == null)
            {
                throw new Exception("FMC buffer is empty.");
            }
            return fmcData;
        }
        public Int32 GetFMCSize()
        {
            return fmcData.Length;
        }
        public void ProcessHeader()
        {
            Loaded = false;

            if (fmcData == null)
            {
                throw new Exception("FMC buffer is empty.");
            }
            
            pElements.Clear();
            MemoryStream ms = new MemoryStream(fmcData);
            BinaryReader reader = new BinaryReader(ms);

            // Reader Header Pointers

            if (reader.ReadInt32() != 0x04)
                throw new Exception("File format not supported.");

            pElements.Add("Description", reader.ReadInt32());

            if (reader.ReadInt32() != 0x01)
                throw new Exception("File format not supported.");

            reader.BaseStream.Position = reader.ReadInt32();

            if (reader.ReadInt32() != 0x04)
                throw new Exception("File format not supported.");

            pElements.Add("Format", reader.ReadInt32());

            if (reader.ReadInt32() != 0x01)
                throw new Exception("File format not supported.");

            reader.BaseStream.Position = reader.ReadInt32();

            if (reader.ReadInt32() != 0x01)
                throw new Exception("File format not supported.");

            Int32 CompatibilityOffset = reader.ReadInt32();
            pElements.Add("Compatibility", CompatibilityOffset);

            reader.BaseStream.Position = CompatibilityOffset;
            reader.BaseStream.Position += reader.ReadByte() + 1;

            // Skip form data size
            reader.BaseStream.Position += 12;

            // Get Element Count
            pElements.Add("ElementCount", (Int32)reader.BaseStream.Position);

            // Skip form element size
            reader.BaseStream.Position += 12;

            // Read Elements
            while (HasRequiredElements() == false && reader.BaseStream.Position < reader.BaseStream.Length)
            {
                reader.BaseStream.Position += ReadElement((Int32)reader.BaseStream.Position);
            }
            Loaded = true;
        }
        private Int32 ReadElement(Int32 Offset)
        {
            MemoryStream ms = new MemoryStream(fmcData);
            BinaryReader reader = new BinaryReader(ms);
            reader.BaseStream.Position = Offset;

            Int32 size = reader.ReadInt32();
            Int32 type = reader.ReadInt32();
            if      (type == 0x01) pElements.Add("FormName", Offset);
            else if (type == 0x21) pElements.Add("Protection1", Offset);
            else if (type == 0x22) pElements.Add("Protection2", Offset);
            else if (type == 0x23) pElements.Add("Protection3", Offset);
            else if (type == 0x31) pElements.Add("Language", Offset);
            else if (type == 0x5B) pElements.Add("PaperSize", Offset);
            else if (type == 0x7E) pElements.Add("JSON", Offset);
            return size;
        }
        private bool HasRequiredElements()
        {
            if (pElements.ContainsKey("FormName") == true &&
                pElements.ContainsKey("Protection1") == true &&
                pElements.ContainsKey("Protection2") == true &&
                pElements.ContainsKey("Protection3") == true &&
                pElements.ContainsKey("Language") == true &&
                pElements.ContainsKey("PaperSize") == true)
                return true;
            else
                return false;
        }
        public string GetDescription()
        {
            if (pElements.ContainsKey("Description") == false)
                throw new Exception("Description property not found in FMC.");

            MemoryStream ms = new MemoryStream(fmcData);
            BinaryReader reader = new BinaryReader(ms);
            reader.BaseStream.Position = pElements["Description"];
            Int32 length = reader.ReadByte();
            return new string(Encoding.ASCII.GetString(fmcData[(pElements["Description"] + 1)..((pElements["Description"] + 1) + length)]));
        }
        public string GetFormat()
        {
            if (pElements.ContainsKey("Format") == false)
                throw new Exception("Format property not found in FMC.");

            MemoryStream ms = new MemoryStream(fmcData);
            BinaryReader reader = new BinaryReader(ms);
            reader.BaseStream.Position = pElements["Format"];
            Int32 length = reader.ReadByte();
            return new string(Encoding.ASCII.GetString(fmcData[(pElements["Format"] + 1)..((pElements["Format"] + 1) + length)]));
        }
        public Int32 GetElementCount()
        {
            if (pElements.ContainsKey("ElementCount") == false)
                throw new Exception("ElementCount property not found in FMC.");

            MemoryStream ms = new MemoryStream(fmcData);
            BinaryReader reader = new BinaryReader(ms);
            reader.BaseStream.Position = pElements["ElementCount"] + 4;
            return reader.ReadInt32();
        }
        public string GetCompatibility()
        {
            if (pElements.ContainsKey("Compatibility") == false)
                throw new Exception("Compatibility property not found in FMC.");

            MemoryStream ms = new MemoryStream(fmcData);
            BinaryReader reader = new BinaryReader(ms);
            reader.BaseStream.Position = pElements["Compatibility"];
            Int32 length = reader.ReadByte();
            return new string(Encoding.ASCII.GetString(fmcData[(pElements["Compatibility"] + 1)..((pElements["Compatibility"] + 1) + length)]));
        }
        public string GetLanguage()
        {
            if (pElements.ContainsKey("Language") == false)
                throw new Exception("Language property not found in FMC.");

            MemoryStream ms = new MemoryStream(fmcData);
            BinaryReader reader = new BinaryReader(ms);
            reader.BaseStream.Position = pElements["Language"];
            Int32 length = reader.ReadInt32() - 14;
            return new string(Encoding.Unicode.GetString(fmcData[(pElements["Language"] + 14)..((pElements["Language"] + 14) + length)]));
        }
        public string GetPaperSize()
        {
            if (pElements.ContainsKey("PaperSize") == false)
                throw new Exception("PaperSize property not found in FMC.");

            MemoryStream ms = new MemoryStream(fmcData);
            BinaryReader reader = new BinaryReader(ms);
            reader.BaseStream.Position = pElements["PaperSize"];
            Int32 length = reader.ReadInt32() - 14;
            return new string(Encoding.Unicode.GetString(fmcData[(pElements["PaperSize"] + 14)..((pElements["PaperSize"] + 14) + length)]));
        }
        public string GetFormName()
        {
            if (pElements.ContainsKey("FormName") == false)
                throw new Exception("FormName property not found in FMC.");

            MemoryStream ms = new MemoryStream(fmcData);
            BinaryReader reader = new BinaryReader(ms);
            reader.BaseStream.Position = pElements["FormName"];
            Int32 length = reader.ReadInt32() - 14;
            return new string(Encoding.Unicode.GetString(fmcData[(pElements["FormName"] + 14)..((pElements["FormName"] + 14) + length)]));
        }
        public string GetID()
        {
            if (pElements.ContainsKey("ID") == false)
                throw new Exception("ID property not found in FMC.");

            MemoryStream ms = new MemoryStream(fmcData);
            BinaryReader reader = new BinaryReader(ms);
            reader.BaseStream.Position = pElements["ID"];
            Int32 length = reader.ReadInt32() - 12;
            return Convert.ToHexString(fmcData[(pElements["ID"] + 12)..((pElements["ID"] + 12) + length)]).ToUpper();
        }
        public byte[] GetIDBytes()
        {
            if (pElements.ContainsKey("ID") == false)
                throw new Exception("ID property not found in FMC.");

            MemoryStream ms = new MemoryStream(fmcData);
            BinaryReader reader = new BinaryReader(ms);
            reader.BaseStream.Position = pElements["ID"];
            Int32 length = reader.ReadInt32() - 12;
            return fmcData[(pElements["ID"] + 12)..((pElements["ID"] + 12) + length)];
        }
        public bool GetProtection1()
        {
            if (pElements.ContainsKey("Protection1") == false)
                throw new Exception("Protection1 property not found in FMC.");

            MemoryStream ms = new MemoryStream(fmcData);
            BinaryReader reader = new BinaryReader(ms);
            reader.BaseStream.Position = pElements["Protection1"] + 12;
            return (reader.ReadByte() != 0 ? true : false);
        }
        public void SetProtection1(bool protect)
        {
            if (pElements.ContainsKey("Protection1") == false)
                throw new Exception("Protection1 property not found in FMC.");

            MemoryStream ms = new MemoryStream(fmcData);
            BinaryWriter writer = new BinaryWriter(ms);
            writer.BaseStream.Position = pElements["Protection1"] + 12;
            writer.Write(protect ? (byte)0x01 : (byte)0x00);
        }
        public bool GetProtection2()
        {
            if (pElements.ContainsKey("Protection2") == false)
                throw new Exception("Protection2 property not found in FMC.");

            MemoryStream ms = new MemoryStream(fmcData);
            BinaryReader reader = new BinaryReader(ms);
            reader.BaseStream.Position = pElements["Protection2"] + 12;
            return (reader.ReadByte() != 0 ? true : false);
        }
        public void SetProtection2(bool protect)
        {
            if (pElements.ContainsKey("Protection2") == false)
                throw new Exception("Protection2 property not found in FMC.");

            MemoryStream ms = new MemoryStream(fmcData);
            BinaryWriter writer = new BinaryWriter(ms);
            writer.BaseStream.Position = pElements["Protection2"] + 12;
            writer.Write(protect ? (byte)0x01 : (byte)0x00);
        }
        public string GetJSON()
        {
            if (pElements.ContainsKey("JSON") == false)
                throw new Exception("JSON property not found in FMC.");

            MemoryStream ms = new MemoryStream(fmcData);
            BinaryReader reader = new BinaryReader(ms);
            reader.BaseStream.Position = pElements["JSON"];
            Int32 length = reader.ReadInt32() - 14;
            return new string(Encoding.Unicode.GetString(fmcData[(pElements["JSON"] + 14)..((pElements["JSON"] + 14) + length)]));
        }
        public bool HasElement(string Element)
        {
            if (pElements.ContainsKey(Element) == true)
                return true;
            else
                return false;
        }
    }
}
