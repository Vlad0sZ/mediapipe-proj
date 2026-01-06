using System;
using System.Collections.Generic;
using System.Text;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using SensorPack.Addons.Mediapipe.Solutions;
using SensorPack.KinectCore.Runtime;
using UnityEngine;
using JointType = SensorPack.KinectCore.Runtime.KinectInterop.JointType;
using TrackingState = SensorPack.KinectCore.Runtime.KinectInterop.TrackingState;
using HandState = SensorPack.KinectCore.Runtime.KinectInterop.HandState;

namespace SensorPack.Addons.Mediapipe
{
    public static class JointMapper
    {
        /// <summary>
        /// Маппинг 1 к 1.
        /// То есть точка Kinect  соответствует точке Mediapipe.
        /// </summary>
        private static readonly Dictionary<JointType, JointTypeMediapipe> KinectToMediapipeJoint = new()
        {
            [JointType.Head] = JointTypeMediapipe.Nose,

            [JointType.ShoulderLeft] = JointTypeMediapipe.LeftShoulder,
            [JointType.ElbowLeft] = JointTypeMediapipe.LeftElbow,
            [JointType.WristLeft] = JointTypeMediapipe.LeftWrist,
            [JointType.HandLeft] = JointTypeMediapipe.LeftWrist,

            [JointType.ShoulderRight] = JointTypeMediapipe.RightShoulder,
            [JointType.ElbowRight] = JointTypeMediapipe.RightElbow,
            [JointType.WristRight] = JointTypeMediapipe.RightWrist,
            [JointType.HandRight] = JointTypeMediapipe.RightWrist,

            [JointType.HipLeft] = JointTypeMediapipe.LeftHip,
            [JointType.KneeLeft] = JointTypeMediapipe.LeftKnee,
            [JointType.AnkleLeft] = JointTypeMediapipe.LeftAnkle,
            [JointType.FootLeft] = JointTypeMediapipe.LeftHeel,

            [JointType.HipRight] = JointTypeMediapipe.RightHip,
            [JointType.KneeRight] = JointTypeMediapipe.RightKnee,
            [JointType.AnkleRight] = JointTypeMediapipe.RightAnkle,
            [JointType.FootRight] = JointTypeMediapipe.RightHeel,

            [JointType.HandTipLeft] = JointTypeMediapipe.LeftIndex,
            [JointType.ThumbLeft] = JointTypeMediapipe.LeftThumb,
            [JointType.HandTipRight] = JointTypeMediapipe.RightIndex,
            [JointType.ThumbRight] = JointTypeMediapipe.RightThumb,
        };

        /// <summary>
        /// Маппинг 1 к 2.
        /// То есть одна точка Kinect соответствует средней точке между двумя точками Mediapipe.
        /// </summary>
        private static readonly Dictionary<JointType, ICollection<JointTypeMediapipe>>
            KinectJointToCalculateMediapipeJoints = new()
            {
                [JointType.SpineBase] = new [] {JointTypeMediapipe.LeftHip, JointTypeMediapipe.RightHip},
                [JointType.Neck] = new [] {JointTypeMediapipe.LeftShoulder, JointTypeMediapipe.RightShoulder},
                [JointType.SpineMid] = new [] {JointTypeMediapipe.LeftHip, JointTypeMediapipe.RightHip, JointTypeMediapipe.LeftShoulder, JointTypeMediapipe.RightShoulder},
            };

        public static bool MapJoints(KinectInterop.SensorData sensorData, ref KinectInterop.BodyFrameData bodyFrame, int defaultID, PoseLandmarkerResult resultCur)
        {
            for (int i = 0; i < sensorData.bodyCount; i++)
            {
                var landmarks = resultCur.poseLandmarks[i];
                var relativeWorldLandmarks = resultCur.poseWorldLandmarks[i];
                bool isTracked = landmarks.landmarks is {Count: 33} && relativeWorldLandmarks.landmarks is {Count: 33};
                
                bodyFrame.bodyData[i].bIsTracked = (short)(isTracked ? 1 : 0);
                bodyFrame.bodyData[i].liTrackingID = defaultID;

                if(!isTracked)
                    continue;
                
                // Заполняем все точки. Преобразовываем (мапим) из Mediapipe Joint в Kinect Joint.
                // Пропускаем SpineMid.
                
                for (int j = 0; j < 25; j++)
                {
                    var jointType = (JointType) j;
                    Vector3 position;
                    Vector3 relativePoint;
                    TrackingState trackingState;

                    if (KinectToMediapipeJoint.TryGetValue(jointType, out var jointTypeMediapipe))
                    {
                        position = GetMediaPipePoint(jointTypeMediapipe);
                        relativePoint = GetRelativeWorldMediaPipePoint(jointTypeMediapipe);
                        trackingState = GetMediaPipeTracked(jointTypeMediapipe);
                    }
                    else if (KinectJointToCalculateMediapipeJoints.TryGetValue(jointType, out var joints))
                    {
                        var tuple = CalculateMidPointFromJoints(joints);
                        position = tuple.pos;
                        relativePoint = tuple.kPos;
                        trackingState = TrackingState.NotTracked;
                        
                        foreach (var joint in joints)
                        {
                            var state = GetMediaPipeTracked(joint);
                            if (state == TrackingState.Tracked)
                            {
                                trackingState = TrackingState.Tracked;
                                break;
                            } 
                            
                            if (state == TrackingState.Inferred && trackingState == TrackingState.NotTracked)
                            {
                                trackingState = TrackingState.Inferred;
                            }
                        }
                    }
                    else
                    {
                        // skip not mapped points.
                        continue;
                    }

                    bodyFrame.bodyData[i].joint[j].kinectPos = position;
                    bodyFrame.bodyData[i].joint[j].position = relativePoint;
                    bodyFrame.bodyData[i].joint[j].trackingState = trackingState;
                }

                var bodyJointData = bodyFrame.bodyData[i].joint[(int) JointType.SpineBase];
                
                // Заполняем дополнительные поля
                bodyFrame.bodyData[i].position = bodyJointData.position;                        // Общая позиция тела в мире (центр таза)
                bodyFrame.bodyData[i].kinectPos = bodyJointData.kinectPos;                      // Позиция в координатах Kinect
                bodyFrame.liRelativeTime = DateTime.UtcNow.Ticks;                               // Временная метка кадра

                bodyFrame.bodyData[i].orientation = Quaternion.identity;                        // Общая ориентация тела (пока не рассчитана)
                bodyFrame.bodyData[i].normalRotation = Quaternion.identity;                     // Нормальное вращение для анимаций (пока не рассчитано)
                bodyFrame.bodyData[i].mirroredRotation = Quaternion.identity;                   // Зеркальное вращение (для отраженного вида)  (пока не рассчитано)

                // bodyFrame.bodyData[i].hipsDirection = (spineMid - spineBase).normalized;        // Направление от таза к груди
                // bodyFrame.bodyData[i].shouldersDirection = (rightShoulder - leftShoulder).normalized; // Направление линии плеч

                //TODO: рассчитать!
                bodyFrame.bodyData[i].leftHandState = HandState.Unknown;                            // Состояние левой руки (открыта/закрыта)
                bodyFrame.bodyData[i].rightHandState = HandState.Unknown;                           // Состояние правой руки
                bodyFrame.bodyData[i].leftHandConfidence = KinectInterop.TrackingConfidence.Low;    // Достоверность отслеживания левой руки
                bodyFrame.bodyData[i].rightHandConfidence = KinectInterop.TrackingConfidence.Low;   // Достоверность отслеживания правой руки
                continue;

                // Получение точки из result.poseLandmarks по типу
                //  Mediapipe выдает координаты в диапазоне [0;1] по x и y, растягиваем по x, сохраняя центрирование.
                // Координата z устаналивается 0.6f. z-координата от Mediapipe не является глубиной, 
                // при этом в KinectManager идёт проверка на значение не меньше 0.5.
                Vector3 GetMediaPipePoint(JointTypeMediapipe joint)
                {
                    var index = (int)joint;
                    if (index >= landmarks.landmarks.Count)
                        return Vector3.zero;
                    
                    var point = landmarks.landmarks[index];
                    // var x = (point.x - 0.5f) * 3f; // меняем 3f - чем больше, тем "шире" можно будет расставлять руки.
                    // var y = 1f - ((0.5f - point.y) * 2f); // меняем 2f - чем больше, тем "выше/ниже" будут руки.
                    // var z = 0.6f; // point.z
                    
                    return new Vector3(point.x, point.y, 0.6f);//point.z);
                    //z-координата от Mediapipe не является глубиной, при этом в KinectManager идёт проверка на значение не меньше 0.5.
                }
                

                // Заполнение bodyFrameData по индексу jointType из усредненных исходных данных от Mediapipe по индексам typeMediapipe
                //Маппинг между типами mediapipe и kinect и усреднение. Например, спина это середина между левым и правым плечом.
                (Vector3 kPos, Vector3 pos) CalculateMidPointFromJoints(ICollection<JointTypeMediapipe> typeMediapipe)
                {
                    var sumRelativeWorld = Vector3.zero;
                    var sum = Vector3.zero;

                    var count = typeMediapipe.Count;
                    foreach (JointTypeMediapipe type in typeMediapipe)
                    {
                        sumRelativeWorld += GetRelativeWorldMediaPipePoint(type);
                        sum += GetMediaPipePoint(type);
                    }

                    var kPos = sumRelativeWorld / count;
                    var pos = sum / count;
                    return (kPos, pos);
                }

                

                // Получение точки из result.poseWorldLandmarks по типу
                // Координата z увеличивается на 0.6f. z-координата от Mediapipe не является глубиной, 
                // при этом в KinectManager идёт проверка на значение не меньше 0.5.
                Vector3 GetRelativeWorldMediaPipePoint(JointTypeMediapipe joint)
                {
                    var index = (int)joint;
                    if (index >= relativeWorldLandmarks.landmarks.Count)
                        return Vector3.zero;
                    
                    var point = relativeWorldLandmarks.landmarks[index];
                    return new Vector3(point.x, -point.y, 0.6f + point.z);
                }

                TrackingState GetMediaPipeTracked(JointTypeMediapipe jointType)
                {
                    var index = (int) jointType;
                    if (index < 0 || index >= landmarks.landmarks.Count)
                        return TrackingState.NotTracked;

                    var landmark = landmarks.landmarks[index];
                    if (!landmark.visibility.HasValue)
                        return TrackingState.Inferred;
                    
                    
                    var confidence = landmark.visibility.Value;
                    
                    if (landmark.presence.HasValue)
                        confidence = Mathf.Min(confidence, landmark.presence.Value);

                    // Если visible или presense больше чем 0.7 - значит точка трекается.
                    if (confidence >= 0.7f)
                        return TrackingState.Tracked;
                    
                    // Иначе скорее всего точка видна частично.
                    if (confidence >= 0.3f)
                        return TrackingState.Inferred;

                    // Иначе вообще точки нет в кадре или ее не видно.
                    return TrackingState.NotTracked;
                }
            }

            return true;
        }

        public static int ColorWidth { get; set; }
        public static int ColorHeight { get; set; }
    }
}