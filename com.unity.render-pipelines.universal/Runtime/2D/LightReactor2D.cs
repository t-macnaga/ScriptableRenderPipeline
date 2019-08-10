using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Experimental.Rendering.Universal
{

    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [AddComponentMenu("Rendering/2D/Light Reactor 2D (Experimental)")]
    public class LightReactor2D : ShadowCasterGroup2D
    {
        [SerializeField] bool m_HasRenderer = false;
        [SerializeField] bool m_UseRendererSilhouette = true;
        [SerializeField] bool m_CastsShadows = true;
        [SerializeField] bool m_SelfShadows = false;
        [SerializeField] int[] m_ApplyToSortingLayers = new int[1];     // These are sorting layer IDs. If we need to update this at runtime make sure we add code to update global lights

        internal ShadowCasterGroup2D m_ShadowCasterGroup = null;
        internal ShadowCasterGroup2D m_PreviousShadowCasterGroup = null;

        [SerializeField] Vector3[] m_ShapePath;
        [SerializeField] int m_ShapePathHash = 0;
        [SerializeField] int m_PreviousPathHash = 0;
        [SerializeField] Mesh m_Mesh;

        internal Mesh mesh => m_Mesh;
        internal Vector3[] shapePath => m_ShapePath;
        internal int shapePathHash { get { return m_ShapePathHash; } set { m_ShapePathHash = value; } }

        Mesh m_ShadowMesh;

        internal int[] applyToSortingLayers => m_ApplyToSortingLayers;

        public bool useRendererSilhouette => m_UseRendererSilhouette;
        public bool selfShadows => m_SelfShadows;
        public bool castsShadows => m_CastsShadows;


        int m_PreviousShadowGroup = 0;
        bool m_PreviousCastsShadows = true;
        Transform m_PreviousParent;


        private void Awake()
        {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.one);

            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                bounds = renderer.bounds;
            }
            else
            {
                Collider2D collider = GetComponent<Collider2D>();
                if (collider != null)
                    bounds = collider.bounds;
            }

            Vector3 relOffset = bounds.center - transform.position;
            if (m_ShapePath == null || m_ShapePath.Length == 0)
                m_ShapePath = new Vector3[] { relOffset + new Vector3(-bounds.extents.x, -bounds.extents.y), relOffset + new Vector3(bounds.extents.x, -bounds.extents.y), relOffset + new Vector3(bounds.extents.x, bounds.extents.y), relOffset + new Vector3(-bounds.extents.x, bounds.extents.y)};

            m_PreviousParent = transform.parent;
        }


        protected void OnEnable()
        {
            if (m_Mesh == null)
            {
                m_Mesh = new Mesh();
                ShadowUtility.GenerateShadowMesh(ref m_Mesh, m_ShapePath);
                m_PreviousPathHash = m_ShapePathHash;
            }

            m_ShadowCasterGroup = null;
        }

        protected void OnDisable()
        {
            LightUtility.RemoveLightReactorFromGroup(this, m_ShadowCasterGroup);
        }


        public void Update()
        {
            Renderer renderer = GetComponent<Renderer>();
            m_HasRenderer = renderer != null;
            if (!m_HasRenderer)
                m_UseRendererSilhouette = false;

            bool rebuildMesh = false;
            rebuildMesh |= LightUtility.CheckForChange(m_ShapePathHash, ref m_PreviousPathHash);

            if (rebuildMesh)
                ShadowUtility.GenerateShadowMesh(ref m_Mesh, m_ShapePath);

            m_PreviousShadowCasterGroup = m_ShadowCasterGroup;
            bool addedToNewGroup = LightUtility.AddToLightReactorToGroup(this, ref m_ShadowCasterGroup);
            if (addedToNewGroup && m_ShadowCasterGroup != null)
            {
                if (m_PreviousShadowCasterGroup == this)
                    ShadowCasterGroup2DManager.RemoveGroup(this);

                LightUtility.RemoveLightReactorFromGroup(this, m_PreviousShadowCasterGroup);
                if (m_ShadowCasterGroup == this)
                    ShadowCasterGroup2DManager.AddGroup(this);
            }



            if (LightUtility.CheckForChange(m_ShadowGroup, ref m_PreviousShadowGroup))
            {
                ShadowCasterGroup2DManager.RemoveGroup(this);
                ShadowCasterGroup2DManager.AddGroup(this);
            }


            if (LightUtility.CheckForChange(m_CastsShadows, ref m_PreviousCastsShadows))
            {
                if(m_CastsShadows)
                    ShadowCasterGroup2DManager.AddGroup(this);
                else
                    ShadowCasterGroup2DManager.RemoveGroup(this);
            }
        }
    }
}
