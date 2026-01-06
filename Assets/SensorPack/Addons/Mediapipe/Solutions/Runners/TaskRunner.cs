// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using UnityEngine;
using Mediapipe.Tasks.Vision.Core;
using Mediapipe.Unity;


namespace SensorPack.Addons.Mediapipe.Solutions.Runners
{
    public abstract class TaskRunner<TTask> : BaseRunner where TTask : BaseVisionTaskApi
    {
        private Coroutine _coroutine;
        protected TTask TaskApi;
        public ImageSource.ImageSource ImageSource;

        public void Initialize(ImageSource.ImageSource imageSource)
        {
            this.ImageSource = imageSource;
            DontDestroyOnLoad(gameObject);
        }

        public override void Play()
        {
            if (_coroutine != null) 
                Stop();
            
            base.Play();
            _coroutine = StartCoroutine(Run());
        }

        public override void Pause()
        {
            base.Pause();
            ImageSource.Pause();
        }

        public override void Resume()
        {
            base.Resume();
            StartCoroutine(ImageSource.Resume());
        }

        public override void Stop()
        {
            base.Stop();
            if (_coroutine != null)
                StopCoroutine(_coroutine);
            
            ImageSource.Stop();
            TaskApi?.Close();
            TaskApi = null;
        }

        protected abstract IEnumerator Run();

        protected static void SetupAnnotationController<T>(global::Mediapipe.Unity.AnnotationController<T> annotationController,
            ImageSource.ImageSource imageSource, bool expectedToBeMirrored = false) where T : HierarchicalAnnotation
        {
            annotationController.isMirrored = expectedToBeMirrored;
            annotationController.imageSize = new Vector2Int(imageSource.textureWidth, imageSource.textureHeight);
        }
    }
}