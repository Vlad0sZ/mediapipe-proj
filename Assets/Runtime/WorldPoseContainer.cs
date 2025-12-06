using System;
using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using UnityEngine;

namespace Runtime
{
    /// <summary>
    /// Абстрактный контейнер для хранения и чтения данных в формате.
    /// </summary>
    /// <typeparam name="TElement">Тип элементов в контейнере для хранения.</typeparam>
    /// <typeparam name="TOutput">Тип форматирования элементов для чтения. </typeparam>
    public abstract class Container<TElement, TOutput>
    {
        // Список элементов.
        private readonly List<TElement> _container;

        protected Container(List<TElement> container) =>
            _container = container;

        public TOutput this[int i]
        {
            get
            {
                if (i < 0 || i > _container.Count)
                    throw new ArgumentOutOfRangeException(nameof(i), "the index was outside the bounds of the array.");

                return Format(_container[i]);
            }
        }

        protected abstract TOutput Format(TElement elem);
    }


    /// <summary>
    /// Контейнер для хранения элементов и возвращения их в формате <see cref="Vector3"/>.
    /// </summary>
    /// <typeparam name="T">Тип элемента.</typeparam>
    public abstract class PoseContainer<T> : Container<T, Vector3>
    {
        protected PoseContainer(List<T> container) : base(container)
        {
        }
    }


    /// <summary>
    /// Контейнер для хранения позиций суставов в нормальных координатах.
    /// </summary>
    public sealed class NormalizedPoseContainer : PoseContainer<NormalizedLandmark>
    {
        public NormalizedPoseContainer(List<NormalizedLandmark> container) : base(container)
        {
        }

        protected override Vector3 Format(NormalizedLandmark elem) =>
            elem.ToVector().Inverse();
    }

    /// <summary>
    /// Контейнер для хранения позиций суставов в мировых координатах.
    /// </summary>
    public sealed class WorldPoseContainer : PoseContainer<Landmark>
    {
        public WorldPoseContainer(List<Landmark> container) : base(container)
        {
        }

        protected override Vector3 Format(Landmark elem) => elem.ToVector().Inverse();
    }
}