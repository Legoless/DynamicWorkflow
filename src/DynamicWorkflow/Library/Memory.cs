using System;
using System.Runtime.Serialization;

namespace DynamicWorkflow.Library
{
    [Serializable()]
    public class Memory
    {
        public Type currentType;
        public object value;
        public bool full = false;

        /*
        public Memory()
        {
        }

        public Memory(SerializationInfo info, StreamingContext ctxt)
        {
            this.currentType = (Type)info.GetValue ("currentType", typeof(Type));
            this.value = (object)info.GetValue ("value", typeof(object));
            this.full = (bool)info.GetValue("full", typeof(bool));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("currentType", this.currentType);
            info.AddValue("value", this.value);
            info.AddValue("full", this.full);
        }*/
    }
}
