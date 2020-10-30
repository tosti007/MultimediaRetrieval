#if Windows
using System;
using System.IO;
using wrapper;

namespace MultimediaRetrieval
{
    public class ANN
    {
        private wrapper.KDTree instance;
        public int K { get; private set; }

        public ANN()
        {
            instance = null;
        }

        ~ANN()
        {
            if (instance != null)
                instance.RemoveKDtree();
        }

        public bool HasData()
        {
            return instance != null;
        }

        public static bool FileExists()
        {
            return File.Exists("kdtree.tree");
        }

        public void Create(FeatureDatabase db, int k_input)
        {
            instance = new wrapper.KDTree();
            K = k_input;
            int npts = db.meshes.Count;
            float[] dataArr = db.ToArray();
            int dim = db.meshes[0].Features.Size;
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
            instance = new wrapper.KDTree();
            K = instance.LoadKDTree();
        }

        public MeshStatistics[] Search(FeatureDatabase db, FeatureVector query, float eps = 0.0f)
        {
            float[] queryArr = query.ToArray();
            int dim = db.meshes[0].Features.Size;
            MeshStatistics[] result = new MeshStatistics[K];
            unsafe
            {
                fixed (float* queryArrPtr = queryArr)
                {
                    int* topIndicesPtr = instance.SearchKDTree(dim, K, queryArrPtr, eps);
                    for (int i = 0; i < K; i++)
                        result[i] = db.meshes[topIndicesPtr[i]];
                }
            }
            return result;
        }
    }
}
#endif
