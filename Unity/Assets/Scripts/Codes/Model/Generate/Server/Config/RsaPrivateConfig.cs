using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class RsaPrivateConfigCategory : ConfigSingleton<RsaPrivateConfigCategory>, IMerge
    {
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, RsaPrivateConfig> dict = new Dictionary<int, RsaPrivateConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<RsaPrivateConfig> list = new List<RsaPrivateConfig>();
		
        public void Merge(object o)
        {
            RsaPrivateConfigCategory s = o as RsaPrivateConfigCategory;
            this.list.AddRange(s.list);
        }
		
		[ProtoAfterDeserialization]        
        public void ProtoEndInit()
        {
            foreach (RsaPrivateConfig config in list)
            {
                config.AfterEndInit();
                this.dict.Add(config.Id, config);
            }
            this.list.Clear();
            
            this.AfterEndInit();
        }
		
        public RsaPrivateConfig Get(int id)
        {
            this.dict.TryGetValue(id, out RsaPrivateConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (RsaPrivateConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, RsaPrivateConfig> GetAll()
        {
            return this.dict;
        }

        public RsaPrivateConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class RsaPrivateConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>私钥</summary>
		[ProtoMember(2)]
		public string Key { get; set; }

	}
}
