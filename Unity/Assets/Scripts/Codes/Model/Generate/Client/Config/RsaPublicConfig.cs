using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class RsaPublicConfigCategory : ConfigSingleton<RsaPublicConfigCategory>, IMerge
    {
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, RsaPublicConfig> dict = new Dictionary<int, RsaPublicConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<RsaPublicConfig> list = new List<RsaPublicConfig>();
		
        public void Merge(object o)
        {
            RsaPublicConfigCategory s = o as RsaPublicConfigCategory;
            this.list.AddRange(s.list);
        }
		
		[ProtoAfterDeserialization]        
        public void ProtoEndInit()
        {
            foreach (RsaPublicConfig config in list)
            {
                config.AfterEndInit();
                this.dict.Add(config.Id, config);
            }
            this.list.Clear();
            
            this.AfterEndInit();
        }
		
        public RsaPublicConfig Get(int id)
        {
            this.dict.TryGetValue(id, out RsaPublicConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (RsaPublicConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, RsaPublicConfig> GetAll()
        {
            return this.dict;
        }

        public RsaPublicConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class RsaPublicConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>公钥</summary>
		[ProtoMember(2)]
		public string Key { get; set; }

	}
}
