#if Windows
using System;
using System.IO;
using wrapper;

namespace MultimediaRetrieval
{
    public class ANN
    {
        private wrapper.KDTree instance;
        private FeatureDatabase _db;
        public int K { get; private set; }

        public ANN(FeatureDatabase db, int k_input)
        {
            instance = new wrapper.KDTree();
            K = k_input;
            _db = db;
            if (db.meshes.Count == 0)
                throw new ArgumentException("Cannot use a FeatureDatabase with no elements for ANN");

        }

        public static bool FileExists()
        {
            return File.Exists("kdtree.tree");
        }

        public void Create()
        {
            int npts = _db.meshes.Count;
            float[] dataArr = _db.ToArray();
            int dim = _db.meshes[0].Features.Size;
            unsafe
            {
                fixed (float* dataArrPtr = dataArr)
                {
                    instance.CreateKDTree(dim, npts, K, dataArrPtr);
                }
            }
        }

        public void Load()
        {
            instance.LoadKDTree();
        }

        public MeshStatistics[] Search(FeatureVector query, float eps = 0.0f)
        {
            float[] queryArr = query.ToArray();
            int dim = _db.meshes[0].Features.Size;
            MeshStatistics[] result = new MeshStatistics[K];
            unsafe
            {
                fixed (float* queryArrPtr = queryArr)
                {
                    int* topIndicesPtr = instance.SearchKDTree(dim, K, queryArrPtr, eps);
                    for (int i = 0; i < K; i++)
                        result[i] = _db.meshes[topIndicesPtr[i]];
                }
            }
            return result;
        }
    }
}
#endif
