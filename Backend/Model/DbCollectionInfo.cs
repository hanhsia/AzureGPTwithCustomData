using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Backend.Model
{

    public class DbCollectionInfo
    {
        [JsonProperty("result")]
        public Result Result { get; set; }=new Result();

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("time")]
        public double Time { get; set; }
    }

    public class Result
    {
        [JsonProperty("status")]
        public string Status { get; set; }= string.Empty;

        [JsonProperty("optimizer_status")]
        public string OptimizerStatus { get; set; }= string.Empty;

        [JsonProperty("vectors_count")]
        public int VectorsCount { get; set; }

        [JsonProperty("indexed_vectors_count")]
        public int IndexedVectorsCount { get; set; }

        [JsonProperty("points_count")]
        public int PointsCount { get; set; }

        [JsonProperty("segments_count")]
        public int SegmentsCount { get; set; }

        [JsonProperty("config")]
        public Config Config { get; set; }=new Config();

        [JsonProperty("payload_schema")]
        public Dictionary<string, object> PayloadSchema { get; set; }=new Dictionary<string, object>();
    }

    public class Config
    {
        [JsonProperty("params")] 
        public Params Params { get; set; } = new Params();

        [JsonProperty("hnsw_config")] 
        public HnswConfig HnswConfig { get; set; } = new HnswConfig();

        [JsonProperty("optimizer_config")] 
        public OptimizerConfig OptimizerConfig { get; set; } = new OptimizerConfig();

        [JsonProperty("wal_config")] 
        public WalConfig WalConfig { get; set; } = new WalConfig();
    }

    public class Params
    {
        [JsonProperty("vectors")] 
        public VectorType VectorType { get; set; } = new VectorType();


        [JsonProperty("shard_number")]
        public int ShardNumber { get; set; }

        [JsonProperty("replication_factor")]
        public int ReplicationFactor { get; set; }

        [JsonProperty("write_consistency_factor")]
        public int WriteConsistencyFactor { get; set; }

        [JsonProperty("on_disk_payload")]
        public bool OnDiskPayload { get; set; }
    }

    public class HnswConfig
    {
        [JsonProperty("m")]
        public int M { get; set; }

        [JsonProperty("ef_construct")]
        public int EfConstruct { get; set; }

        [JsonProperty("full_scan_threshold")]
        public int FullScanThreshold { get; set; }

        [JsonProperty("max_indexing_threads")]
        public int MaxIndexingThreads { get; set; }
    }

    public class OptimizerConfig
    {
        [JsonProperty("deleted_threshold")]
        public double DeletedThreshold { get; set; }

        [JsonProperty("vacuum_min_vector_number")]
        public int VacuumMinVectorNumber { get; set; }

        [JsonProperty("default_segment_number")]
        public int DefaultSegmentNumber { get; set; }

        [JsonProperty("max_segment_size")]
        public int? MaxSegmentSize { get; set; }

        [JsonProperty("memmap_threshold")]
        public int? MemmapThreshold { get; set; }

        [JsonProperty("indexing_threshold")]
        public int IndexingThreshold { get; set; }

        [JsonProperty("flush_interval_sec")]
        public int FlushIntervalSec { get; set; }

        [JsonProperty("max_optimization_threads")]
        public int MaxOptimizationThreads { get; set; }
    }

    public class WalConfig
    {
        [JsonProperty("wal_capacity_mb")]
        public int WalCapacityMb { get; set; }

        [JsonProperty("wal_segments_ahead")]
        public int WalSegmentsAhead { get; set; }
    }
}
