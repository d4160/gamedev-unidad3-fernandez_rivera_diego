#region Using statements

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Bitgem.VFX.StylisedWater
{
    [AddComponentMenu("Bitgem/Water  Volume (Box)")]
    public class WaterVolumeBox : WaterVolumeBase
    {
        #region Public fields

        public Vector3 Dimensions = Vector3.zero;

        #endregion

        #region Public methods

        protected override void GenerateTiles(ref bool[,,] _tiles)
        {
            int dimX = Mathf.FloorToInt(Dimensions.x);
            int dimY = Mathf.FloorToInt(Dimensions.y);
            int dimZ = Mathf.FloorToInt(Dimensions.z);

            for (int x = 0; x < dimX; x++)
            {
                for (int z = 0; z < dimZ; z++)
                {
                    for (int y = 0; y < dimY; y++)
                    {
                        _tiles[x, y, z] = true;
                    }
                }
            }
        }

        public override void Validate()
        {
            // keep values sensible
            Dimensions.x = Mathf.Clamp(Dimensions.x, 1, MAX_TILES_X);
            Dimensions.y = Mathf.Clamp(Dimensions.y, 1, MAX_TILES_Y);
            Dimensions.z = Mathf.Clamp(Dimensions.z, 1, MAX_TILES_Z);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;

            Vector3 localSize = Dimensions * TileSize;
            Vector3 localCenter = localSize * 0.5f;

            Gizmos.DrawWireCube(localCenter, localSize);

            Gizmos.matrix = Matrix4x4.identity; // reset
        }

        #endregion
    }
}