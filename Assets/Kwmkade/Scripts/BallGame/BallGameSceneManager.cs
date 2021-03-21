using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kwmkade.CrossPlatformInput;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BallGame
{
    public class BallGameSceneManager : MonoBehaviour
    {
        [SerializeField, NotNull]
        private JoystickHandler _joystickHandler;

        [SerializeField, NotNull]
        private GameObject _ballGameObject;

        [SerializeField]
        private float _forceCoefficient = 100.0f;

        [SerializeField, NotNull]
        private Camera _mainCamera;

        [SerializeField, NotNull]
        private ButtonHandler _cameraButtonHandler;

        [SerializeField, NotNull]
        private ButtonHandler _pauseButtonHandler;

        private Pauser _pauser = new Pauser();

        private CameraController _cameraController;

        public Kwmkade.UI.Dialog.CommonDialog dialog;

        private bool _isCompleted;


        public void OpenPauseDialog()
        {
            var btnContinue = new Kwmkade.UI.Dialog.DialogButton.ActionButton("Continue", () => { _pauser.Resume(); });

            var btnRetry = new Kwmkade.UI.Dialog.DialogButton.ActionButton("Retry", ReloadScecne);

            var btnHome = new Kwmkade.UI.Dialog.DialogButton.ActionButton("Home", () => { Debug.Log("click ok"); });

            Kwmkade.UI.Dialog.DialogButton.ActionButton[] buttons = { btnContinue, btnRetry, btnHome };

            dialog.ShowDialog("Pause", "", buttons, () => { Debug.Log("closed dialog."); }, true);
        }

        public void OpenCompleteDialog()
        {
            var btnHome = new Kwmkade.UI.Dialog.DialogButton.ActionButton("Home", () => { Debug.Log("click ok"); });

            Kwmkade.UI.Dialog.DialogButton.ActionButton[] buttons = { btnHome };

            dialog.ShowDialog("Complete!", "", buttons, () => { Debug.Log("closed dialog."); }, true);
        }

        void Start()
        {
            _pauser.Reset();
            _pauser.Add(_ballGameObject);

            _cameraController = new CameraController(_mainCamera, () => { return _ballGameObject.transform.position; });

            _isCompleted = false;
        }

        void Update()
        {
            var input = new Vector3(_joystickHandler.InputVector.x, 0, _joystickHandler.InputVector.y);
            input = Quaternion.Euler(0, _cameraController.RotateAngle, 0) * input;
            _ballGameObject.GetComponent<Rigidbody>().AddForce(input * _forceCoefficient * Time.deltaTime);
        }

        void LateUpdate()
        {
            if (_cameraButtonHandler.IsClicked)
            {
                _cameraController.RequestRotate(90);
            }
            else if (_pauseButtonHandler.IsClicked)
            {
                if (!_pauser.IsPause)
                {
                    _pauser.Pause();
                }
                OpenPauseDialog();
            }

            ResetButtonHandlers();

            _cameraController.Update(Time.deltaTime);

            if (_isCompleted)
            {
                return;
            }

            if (IsCollisionWith(CollisionTarget.Dead))
            {
                ReloadScecne();
            }
            else if (IsCollisionWith(CollisionTarget.Goal))
            {                
                OpenCompleteDialog();

                _cameraController.RequestPlayingGoal();

                var rd = _ballGameObject.GetComponent<Rigidbody>();
                rd.isKinematic = true;

                _isCompleted = true;
            }
        }

        private void ResetButtonHandlers()
        {
            _cameraButtonHandler.IsClicked = false;
            _pauseButtonHandler.IsClicked = false;
        }

        private bool IsCollisionWith(CollisionTarget target)
        {
            return _ballGameObject.GetComponent<BallCollisionChecker>()?.IsCollisionWith(target) ?? false;
        }

        private void ReloadScecne()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        #region ----- CameraController -----
        class CameraController
        {
            private Camera _camera;

            private Func<Vector3> _followPositionRetriever;

            private int _requestedRotateAngle;

            private Vector3 _followeeOffset = Vector3.zero;

            public float RotateAngle { get; private set; }

            private float a = 0f;

            private Vector3 _forceTargetPosition = Vector3.zero;

            public CameraController(Camera camera, Func<Vector3> followPositionRetriever)
            {
                this._camera = camera;
                this._followPositionRetriever = followPositionRetriever;
                RotateAngle = 0f;
                this._requestedRotateAngle = 0;
            }


            public void Update(float dt)
            {
                // NOTE: 初回の初期化
                if (_followeeOffset == Vector3.zero)
                {
                    _followeeOffset = _camera.transform.position - _followPositionRetriever();
                }


                if (_forceTargetPosition != Vector3.zero)
                {
                    if ((_forceTargetPosition - _camera.transform.position).magnitude > 1)
                    {
                        var ballPos = _followPositionRetriever();
                        _camera.transform.position += (_forceTargetPosition - ballPos) * dt;
                    }
                    return;
                }

                _camera.transform.position = _followPositionRetriever() + _followeeOffset;

                if (_requestedRotateAngle > 0)
                {
                    var deltaAngle = Mathf.RoundToInt(dt * 1000f);
                    deltaAngle = Mathf.Min(deltaAngle, _requestedRotateAngle);

                    this._camera.transform.RotateAround(_followPositionRetriever(), Vector3.up, deltaAngle);
                    _followeeOffset = _camera.transform.position - _followPositionRetriever();
                    RotateAngle += (float)deltaAngle;
                    _requestedRotateAngle -= deltaAngle;
                }
            }

            public void RequestRotate(int angle)
            {
                this._requestedRotateAngle += angle;
            }

            public void RequestPlayingGoal()
            {
                var cameraPos = _camera.transform.position;
                var ballPos = _followPositionRetriever();
                this._forceTargetPosition = (cameraPos - ballPos) * 5f + cameraPos;
            }
        }
        #endregion

        #region ----- Pauser -----
        private class Pauser
        {
            private class Velocity3DTmp : MonoBehaviour
            {
                private Vector3 _angularVelocity;
                private Vector3 _velocity;

                public Vector3 AngularVelocity
                {
                    get { return _angularVelocity; }
                }
                public Vector3 Velocity
                {
                    get { return _velocity; }
                }

                public void Set(Rigidbody rigidbody)
                {
                    _angularVelocity = rigidbody.angularVelocity;
                    _velocity = rigidbody.velocity;
                }
            }

            private List<GameObject> _targets = new List<GameObject>();

            public bool IsPause { get; private set; }

            public void Add(GameObject obj)
            {
                _targets.Add(obj);
            }

            public void Reset()
            {
                _targets.Clear();
            }

            public void Pause()
            {
                if (IsPause)
                {
                    return;
                }

                foreach (var obj in _targets)
                {
                    Pause(obj);
                }

                IsPause = true;
            }

            public void Resume()
            {
                if (!IsPause)
                {
                    return;
                }

                foreach (var obj in _targets)
                {
                    Resume(obj);
                }

                IsPause = false;
            }

            private static void Pause(GameObject obj)
            {
                if (!obj.activeSelf)
                {
                    return;
                }

                var rd = obj.GetComponent<Rigidbody>();
                if (rd != null)
                {
                    obj.AddComponent<Velocity3DTmp>().Set(rd);
                    rd.isKinematic = true;
                }

                var animator = obj.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.speed = 0;
                }
            }

            private static void Resume(GameObject obj)
            {
                if (!obj.activeSelf)
                {
                    return;
                }

                var tmp = obj.GetComponent<Velocity3DTmp>();
                var rb = obj.GetComponent<Rigidbody>();
                if (tmp != null && rb != null)
                {
                    rb.velocity = obj.GetComponent<Velocity3DTmp>().Velocity;
                    rb.angularVelocity = obj.GetComponent<Velocity3DTmp>().AngularVelocity;
                    rb.isKinematic = false;

                    GameObject.Destroy(obj.GetComponent<Velocity3DTmp>());
                }

                var animator = obj.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.speed = 1f;
                }
            }
        }
        #endregion
    }
}

