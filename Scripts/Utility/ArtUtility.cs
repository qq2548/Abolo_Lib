using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AboloLib
{
    public enum RendererType
    {
        [Tooltip("UI渲染")]
        Image,
        [Tooltip("Sprite渲染")]
        SpriteRenderer,
    }

    public static class ArtUtility
    {
        public const string IdleName = "ani_idle";
        public const string UnlockName = "ani_unlock";

        public static string WarningLog = "AboloWarning>>>";
        public static AnimationCurve IncreaseLinearCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
        public static AnimationCurve DecreaseLinearCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 0.0f);


        public static  void PlayFx(Transform parent , GameObject fxInstance)
        {
            fxInstance.SetActive(true);
            fxInstance.transform.SetParent(parent);
            fxInstance.transform.localScale = Vector3.one;
            fxInstance.transform.localPosition = Vector3.zero;
        }

        public static void PlayFx(Transform parent, GameObject fxInstance , Vector3 offset)
        {
            PlayFx(parent, fxInstance);
            fxInstance.transform.localPosition += offset;
        }

        /// <summary>
        /// 通过脚本设置粒子系统自定义顶点数据，需要disable掉 Custom Data模块才会生效
        /// </summary>
        /// <param name="particleSystem"></param>
        /// <param name="value">自定义数据，配合自定义shader使用</param>
        public static void SetParticleCustomData(ParticleSystem particleSystem, Vector4 value)
        {
            List<Vector4> data = new List<Vector4>();
            particleSystem.GetCustomParticleData(data, ParticleSystemCustomData.Custom1);
            for (int i = 0; i < data.Count; i++)
            {
                data[i] = value;
            }
            particleSystem.SetCustomParticleData(data, ParticleSystemCustomData.Custom1);
        }


        public static void SetParticleLifetime(ParticleSystem particleSystem, float maxLife , float remainLife)
        {
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleSystem.particleCount];
            particleSystem.GetParticles(particles);
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].startLifetime = maxLife; ;
                particles[i].remainingLifetime = remainLife; ;
            }
            particleSystem.SetParticles(particles);
        }

        public static void SetParticleRemainLife(ParticleSystem _particleSystem , float weight = 1.0f)
        {
            var _particles = new ParticleSystem.Particle[_particleSystem.particleCount];
            _particleSystem.GetParticles(_particles);
            for (int i = 0; i < _particles.Length; i++)
            {
                float t = _particles[i].remainingLifetime;

                _particles[i].remainingLifetime = (_particles.Length - i) * weight;
            }
            _particleSystem.SetParticles(_particles);
        }

        public static void SetParticleRemainLife(ParticleSystem _particleSystem, float remianLife , float weight = 1.0f)
        {
            var _particles = new ParticleSystem.Particle[_particleSystem.particleCount];
            _particleSystem.GetParticles(_particles);
            for (int i = 0; i < _particles.Length; i++)
            {
                float t = _particles[i].remainingLifetime;

                _particles[i].remainingLifetime = remianLife * weight;
            }
            _particleSystem.SetParticles(_particles);
        }

        //配置粒子系统为发射之前的渲染层级，发射器的初始生命周期
        public static void ConfigurePaticleSystem(ParticleSystem ps, int index, float startLifeTime , int maxSortingLayerId)
        {
            var main = ps.main;
            main.startLifetime = startLifeTime;
            int so = maxSortingLayerId - index;
            ps.GetComponent<Renderer>().sortingOrder = so;
        }

        //配置粒子系统,发射频率值，发射器替换mesh
        public static void ConfigurePaticleSystem(ParticleSystem ps , Mesh mesh, float amount = 1.0f)
        {
            var em = ps.emission;

            em.rateOverTime = 100.0f * amount;

            var mshape = ps.shape;
            mshape.mesh = mesh;
        }

        public static void PlayParticles(ParticleSystem[] particles)
        {
            foreach (var ps in particles)
            {
                if (!ps.gameObject.activeInHierarchy)
                {
                    ps.gameObject.SetActive(true);
                }
                var em = ps.emission;
                em.enabled = true;
                ps.Play();
            }
        }

        public static void StopParticles(ParticleSystem[] particles)
        {
            foreach (var ps in particles)
            {
                //var em = ps.emission;
                //em.enabled = false;
                ps.Stop();
            }
        }

        public static void DisableObjects<T>(T[] objects) where T: Component
        {
            foreach (var item in objects)
            {
                item.gameObject.SetActive(false);
            }
        }

        public static void EnableObjects<T>(T[] objects) where T : Component
        {
            foreach (var item in objects)
            {
                item.gameObject.SetActive(true);
            }
        }

        //粒子特效关闭之前，先停止发射粒子，并返回一个最大的延迟时间
        public static float ShutdownParticleSystem(ParticleSystem[] particleSystems)
        {
            float maxDelay = 0.0f;
            for (int i = 0; i < particleSystems.Length; i++)
            {
                //var main = particleSystems[i].main;
                maxDelay = Mathf.Max(maxDelay, particleSystems[i].main.startLifetime.constantMax);
                var emit = particleSystems[i].emission;
                emit.enabled = false;
            }
            return maxDelay;
        }

        //图片 transform 朝向目标 transform
        public static void LookAtTarget(Transform transform , Transform target)
        {
            transform.LookAt(target);
        }

        public static Vector2Int GetGridCoord
            
            (Vector3 originalPos , Vector3 mouseWorldPos ,Vector2 cellSize)
        {
            Vector2Int coord = new Vector2Int();
            Vector3 localMousePos = mouseWorldPos - originalPos;
            coord.x = Mathf.FloorToInt(localMousePos.x / cellSize.x);
            coord.y = Mathf.FloorToInt(localMousePos.y/ cellSize.y);
            return coord;
        }

        public static T[] ArrayRemoveObject<T>(T item , T[] array )
        {
            List<T> list = new List<T>();
            list.AddRange(array);
            list.Remove(item);
            
            T[] result = list.ToArray();
            return result;
        }


        public static Mesh CreateRectMesh(Vector2 scale, Vector2 pivot)
        {
            Mesh m = new Mesh();
            Vector3 offset = new Vector3(pivot.x, pivot.y, 0.0f);
            //这里我们创建一个正方形网格，所以需要4个顶点、4个UV点和6条边
            Vector3[] vertices = new Vector3[4];
            //Vector2[] uv = new Vector2[4];
            int[] triangles = new int[6];
            //声明顶点的位置
            vertices[0] = (new Vector3(0.0f, 1.0f, 0.0f) - offset) * scale;
            vertices[1] = (new Vector3(1.0f, 1.0f, 0.0f) - offset) * scale;
            vertices[2] = (new Vector3(0.0f, 0.0f, 0.0f) - offset) * scale;
            vertices[3] = (new Vector3(1.0f, 0.0f, 0.0f) - offset) * scale;
            //声明UV的位置,这个脚本只要网格做发射器不需要声明UV和赋值
            //uv[0] = new Vector2(0, 1);
            //uv[1] = new Vector2(1, 1);
            //uv[2] = new Vector2(0, 0);
            //uv[3] = new Vector2(1, 0);
            //声明三角边，这里三角边是根据上面的顶点来进行连接的，每三个顶点构成一个三角边
            //这里赋值对应 vertices[]数组中的顶点
            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            triangles[3] = 2;
            triangles[4] = 1;
            triangles[5] = 3;

            m.vertices = vertices;
            m.triangles = triangles;
            // m.uv = uv;

            return m;
        }
        public static Sprite CreateSprite(Texture2D tex, Vector2 size)
        {
            return Sprite.Create(tex, new Rect(0f, 0f, size.x, size.y), Vector2.one * 0.5f);
        }

        public static Mesh CreateCombinedMesh(Transform root , Matrix4x4 matrix)
        {
            Mesh combine = new Mesh();
            var meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
            var spriteRenderers = root.GetComponentsInChildren<SpriteRenderer>(true);

#if _ARTEST_PRESENTATION
            if (meshFilters.Length < 1 && spriteRenderers.Length == 1)
            {
                if (root.TryGetComponent<MeshCollider>(out MeshCollider mc))
                {
                    Debug.LogError(WarningLog + root.parent.name + "--此节点只有1个SpriteRenderer且没有MeshRenderer，生成Mes如果作为MeshCollider使用则无效");
                }
            }
#endif

            //过滤掉特效和阴影类的片面
            List<SpriteRenderer> spr_renderers = new List<SpriteRenderer>();
            spr_renderers.Clear();
            foreach (var spr in spriteRenderers)
            {
                if (!spr.name.Contains("shadow") && !spr.name.Contains("light") && spr.sprite != null)
                {
                    spr_renderers.Add(spr);
                }
            }

            var combineInstances = new CombineInstance[meshFilters.Length + spr_renderers.Count];

            if (meshFilters.Length > 0)
            {
                for (int a = 0; a < meshFilters.Length; a++)
                {
                    Mesh m = new Mesh();
                    m.vertices = meshFilters[a].mesh.vertices;
                    m.triangles = meshFilters[a].mesh.triangles;
                    combineInstances[a].mesh = m;
                    combineInstances[a].transform = matrix * meshFilters[a].transform.localToWorldMatrix;
                }
            }



            if (spr_renderers.Count > 0)
            {
                for (int b = 0; b < spr_renderers.Count; b++)
                {
                    Rect rect;
                    if(spr_renderers[b].sprite != null)
                    {
                        rect = spr_renderers[b].sprite.rect;
                        Vector2 scale = new Vector2(rect.width * 0.01f, rect.height * 0.01f);
                        Mesh m = CreateRectMesh(scale, spr_renderers[b].sprite.pivot / new Vector2(rect.width, rect.height));
#if _ARTEST_PRESENTATION
                        //Debug.Log(spr_renderers[b].transform.GetComponentInParent<Animator>(true).transform.parent.name+"-------"+
                        //    spr_renderers[b].gameObject.name + spr_renderers[b].sprite.pivot / new Vector2(rect.width, rect.height));
#endif
                        combineInstances[b + meshFilters.Length].mesh = m;
                        combineInstances[b + meshFilters.Length].transform = matrix * spr_renderers[b].transform.localToWorldMatrix;
                    }
                    else
                    {
#if _ARTEST_PRESENTATION
                        var dec = spr_renderers[b].GetComponentInParent<DecorationAnim>();
                        Debug.LogError("空引用Sprite："+spr_renderers[b].gameObject.name + "----" + dec.name + "----" + dec.transform.parent.name);
#endif
                    }
                }
            }
            combine.CombineMeshes(combineInstances, true);

            return combine;
        }

       static readonly Vector2Int[] GridDirections =
             {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
             };
        /// <summary>
        /// 获取范围最外圈的所有网格
        /// </summary>
        /// <param name="coord">中心坐标</param>
        /// <param name="range">范围</param>
        /// <returns></returns>
        public static List<Vector2Int> GetOutterNeighborCells(Vector2Int coord, int range)
        {
            if (range < 0)
            {
                return null;
            }
            List<Vector2Int> neighbors = new List<Vector2Int>();
            Vector2Int currVec = new Vector2Int(coord.x - range, coord.y - range);
            neighbors.Add(currVec);
            for (int i = 0; i < 8 * range - 1; i++)
            {
                Vector2Int curDirr = GridDirections[i / (range * 2)];//更新方向
                currVec = currVec + curDirr;//填入数值
                neighbors.Add(currVec);
            }
            Vector2Int v = neighbors[0];
            neighbors.RemoveAt(0);
            neighbors.Add(v);
            return neighbors;
        }

        /// <summary>
        /// 从 Resource 实例化 Sprite 
        /// </summary>
        /// <param name="resPath">资源路径</param>
        /// <returns></returns>
        public static Sprite InstantiateSpriteFromResource(string resPath)
        {
            Texture2D tex = Resources.Load<Texture2D>(resPath);
            if (tex == null)
            {
                Debug.LogError($"{resPath} 此路径资源不存在，无法加载");
                return null;
            }
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        }

        /// <summary>
        /// 重置 transform 本地坐标和缩放参数
        /// </summary>
        public static void ResetTransform(Transform target)
        {
            target.localPosition = Vector3.zero;
            target.localScale = Vector3.one;
        }

        //获取椭圆边缘指定角度位置
        public static Vector2 GetPointOnEllipse(Vector2 rect, float degree)
        {
            float x = rect.x * Mathf.Cos(degree * Mathf.Deg2Rad);
            float y = rect.y * Mathf.Sin(degree * Mathf.Deg2Rad);
            return new Vector2(x, y);
        }
        //获取椭圆内部随机位置
        public static Vector2 RandomInsideEllipse(Vector2 maxRange)
        {
            float x = UnityEngine.Random.Range(-maxRange.x * 0.5f, maxRange.x * 0.5f);
            float y = UnityEngine.Random.Range(-maxRange.y * 0.5f, maxRange.y * 0.5f);
            float degree = UnityEngine.Random.Range(0f, 360f);
            return GetPointOnEllipse(new Vector2(x, y), degree);
        }

        public static void DrawEllipse(Vector3 center, Vector2 size)
        {
            //绘制椭圆
            Gizmos.color = Color.magenta;

            int segments = 100;
            float deltaAngle = 360f / segments;


            Vector3[] vertices = new Vector3[segments];
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 ellipsePos = GetPointOnEllipse(new Vector2(size.x, size.y), deltaAngle * i);
                Vector3 pos = new Vector3(ellipsePos.x, ellipsePos.y, 0.0f) + center;
                vertices[i] = pos;
            }
            for (int i = 0; i < vertices.Length - 1; i++)
            {
                Gizmos.DrawLine(vertices[i], vertices[i + 1]);
            }
            Gizmos.DrawLine(vertices[0], vertices[vertices.Length - 1]);
        }

        #region Animation Functions
        /// <summary>
        /// 浮点数曲线采样方法
        /// </summary>
        /// <param name="timer">计时器，进度</param>
        /// <param name="animationCurve">动画曲线</param>
        /// <param name="range">取样范围</param>
        /// <returns></returns>
        public static float SingleThreshlod(float timer, AnimationCurve animationCurve, Vector2 range)
        {
            float f = Mathf.Lerp(range.x, range.y, animationCurve.Evaluate(timer));
            return f;
        }

        /// <summary>
        /// 协程淡入动画
        /// </summary>
        /// <param name="inputColor">起始颜色值</param>
        /// <param name="timer">计时器，进度</param>
        public static Color FadeIn(Color inputColor , float timer , AnimationCurve curve = null)
        {
            if (curve == null) curve = IncreaseLinearCurve;
            return new Color(inputColor.r, inputColor.g, inputColor.b, curve.Evaluate(timer));
        }

        /// <summary>
        /// 协程淡出动画
        /// </summary>
        /// <param name="inputColor">起始颜色值</param>
        /// <param name="timer">计时器，进度</param>
        public static Color FadeOut(Color inputColor, float timer, AnimationCurve curve = null)
        {
            if (curve == null) curve = DecreaseLinearCurve;
            return new Color(inputColor.r, inputColor.g, inputColor.b, curve.Evaluate(timer));
        }
        /// <summary>
        /// 协程移动入场动画，从上方1单位的位置落下
        /// </summary>
        /// <param name="timer">计时器，进度</param>
        public static Vector3 Move(Vector3 from , Vector3 to , float timer, AnimationCurve curve = null)
        {
            if (curve == null) curve = IncreaseLinearCurve;
            return Vector3.Lerp(from, to, curve.Evaluate(timer));
        }

        /// <summary>
        /// 协程弹出动画
        /// </summary>
        /// <param name="timer">计时器，进度</param>
        public static Vector3 Pop(float timer, AnimationCurve curve = null)
        {
            if (curve == null) curve = IncreaseLinearCurve;
            return Vector3.one * curve.Evaluate(timer);
        }
        /// <summary>
        /// 协程弹出动画
        /// </summary>
        /// <param name="timer">计时器，进度</param>
        /// <param name="curve1">LocalScale值X轴的变化曲线</param>
        /// <param name="curve2">LocalScale值Y轴的变化曲线</param>
        /// <param name="curve3">LocalScale值Z轴的变化曲线</param>
        public static Vector3 Pop( float timer, AnimationCurve curve1 , AnimationCurve curve2, AnimationCurve curve3 = null)
        {
            float z_axis;
            if (curve3 == null)
            {
                z_axis = 1.0f;
            }
            else
            {
                z_axis = curve3.Evaluate(timer);
            }
            return new Vector3(curve1.Evaluate(timer) , curve2.Evaluate(timer) , z_axis);
        }

        /// <summary>
        /// 协程缩小动画
        /// </summary>
        /// <param name="timer">计时器，进度</param>
        public static Vector3 Shrink(float timer, AnimationCurve curve = null)
        {
            if (curve == null) curve = DecreaseLinearCurve;
            return Vector3.one * curve.Evaluate(timer);
        }
        #endregion
    }
}
