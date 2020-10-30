#pragma once

using namespace System;

#include "ANN/ANN.h"
#include <iostream>
#include <fstream>

namespace wrapper {
	public ref class KDTree
	{
	private:
		ANNkd_tree* kdTree;
	public:
		void CreateKDTree(int dim, int npts, int k, float data[])
		{
			ANNpointArray		dataPts;				// data points

			dataPts = annAllocPts(npts, dim);			// allocate data points

			for (int i = 0; i < npts; i++)				// set data points
				for (int j = 0; j < dim; j++)
					dataPts[i][j] = data[i * dim + j];

			kdTree = new ANNkd_tree(		// build search structure
				dataPts,					// the data points
				npts,						// number of points
				dim);						// dimension of space

			std::ofstream file;             // write the tree to file
			file.open("kdtree.tree");
			file << k << std::endl;
			kdTree->Dump(ANNtrue, file);
			file.close();
		}

		int LoadKDTree()
		{
			std::ifstream file;
			int k;

			file.open("kdtree.tree");
  			file >> n;
			std::cin << file.peek() != '\n';
			kdTree = new ANNkd_tree(file);
			file.close();

			return k
		}

		int* SearchKDTree(int dim, int k, float query[], float eps)
		{
			ANNpoint			queryPt;				// query point
			ANNidxArray			nnIdx;					// near neighbor indices
			ANNdistArray		dists;					// near neighbor distances

			queryPt = annAllocPt(dim);					// allocate query point
			nnIdx = new ANNidx[k];						// allocate near neigh indices
			dists = new ANNdist[k];						// allocate near neighbor dists

			for (int i = 0; i < dim; i++) //Read query points
				queryPt[i] = query[i];

			kdTree->annkSearch(					// search
				queryPt,						// query point
				k,								// number of near neighbors
				nnIdx,							// nearest neighbors (returned)
				dists,							// distance (returned)
				eps);							// error bound

			int* result = new int[k];
			for (int i = 0; i < k; i++)         // Put the NN IDs into the result.
				result[i] = nnIdx[i];

			delete[] queryPt;
			delete[] nnIdx;						// clean things up
			delete[] dists;
			return result;
		}

		void RemoveKDtree()
		{
			delete kdTree;
			annClose();							// done with ANN
		}

		int* RunKDtree(int dim, int npts, int k, float data[], float query[], float eps)
		{
			ANNpointArray		dataPts;				// data points
			ANNpoint			queryPt;				// query point
			ANNidxArray			nnIdx;					// near neighbor indices
			ANNdistArray		dists;					// near neighbor distances

			queryPt = annAllocPt(dim);					// allocate query point
			dataPts = annAllocPts(npts, dim);			// allocate data points
			nnIdx = new ANNidx[k];						// allocate near neigh indices
			dists = new ANNdist[k];						// allocate near neighbor dists

			for (int i = 0; i < npts; i++)
				for (int j = 0; j < dim; j++)
					dataPts[i][j] = data[i * dim + j];

			kdTree = new ANNkd_tree(					// build search structure
				dataPts,					// the data points
				npts,						// number of points
				dim);						// dimension of space

			for (int i = 0; i < dim; i++) //Read query points
				queryPt[i] = query[i];

			kdTree->annkSearch(					// search
				queryPt,						// query point
				k,								// number of near neighbors
				nnIdx,							// nearest neighbors (returned)
				dists,							// distance (returned)
				eps);							// error bound

			int* result = new int[k];
			for (int i = 0; i < k; i++)         // Put the NN IDs into the result.
				result[i] = nnIdx[i];

			delete[] nnIdx;						// clean things up
			delete[] dists;
			delete kdTree;
			annClose();							// done with ANN
			return result;
		}
	};
}
