using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Windows;

public class AIController : MonoBehaviour {
    [SerializeField]
    Plane plane;  // 비행체(비행기) 오브젝트에 대한 레퍼런스

    [SerializeField]
    float steeringSpeed;  // 스티어링 속도

    [SerializeField]
    float minSpeed;  // 비행 최소 속도

    [SerializeField]
    float maxSpeed;  // 비행 최대 속도

    [SerializeField]
    float recoverSpeedMin;  // 속도 회복 최소값

    [SerializeField]
    float recoverSpeedMax;  // 속도 회복 최대값

    [SerializeField]
    LayerMask groundCollisionMask;  // 지면과 충돌 여부를 검사하기 위한 레이어 마스크

    [SerializeField]
    float groundCollisionDistance;  // 지면과 충돌 여부를 검사하는 거리

    [SerializeField]
    float groundAvoidanceAngle;  // 지면 회피 각도

    [SerializeField]
    float groundAvoidanceMinSpeed;  // 지면 회피 최소 속도

    [SerializeField]
    float groundAvoidanceMaxSpeed;  // 지면 회피 최대 속도

    [SerializeField]
    float pitchUpThreshold;  // 기체가 위로 회전하는 기준 각도

    [SerializeField]
    float fineSteeringAngle;  // 섬세한 스티어링을 위한 각도

    [SerializeField]
    float rollFactor;  // 기체 회전 시 롤링에 대한 가중치

    [SerializeField]
    float yawFactor;  // 기체 회전 시 요우에 대한 가중치

    [SerializeField]
    bool canUseMissiles;  // 미사일 사용 가능 여부

    [SerializeField]
    bool canUseCannon;  // 대포 사용 가능 여부

    [SerializeField]
    float missileLockFiringDelay;  // 미사일 잠금 후 발사까지의 딜레이

    [SerializeField]
    float missileFiringCooldown;  // 미사일 발사 후의 쿨다운

    [SerializeField]
    float missileMinRange;  // 미사일 최소 사거리

    [SerializeField]
    float missileMaxRange;  // 미사일 최대 사거리

    [SerializeField]
    float missileMaxFireAngle;  // 미사일 발사 최대 각도

    [SerializeField]
    float bulletSpeed;  // 포탄(미사일, 대포 등)의 속도

    [SerializeField]
    float cannonRange;  // 대포의 사거리

    [SerializeField]
    float cannonMaxFireAngle;  // 대포 발사 최대 각도

    [SerializeField]
    float cannonBurstLength;  // 대포 연사 길이

    [SerializeField]
    float cannonBurstCooldown;  // 대포 연사 후의 쿨다운

    [SerializeField]
    float minMissileDodgeDistance;  // 미사일 회피 최소 거리

    [SerializeField]
    float reactionDelayMin;  // 반응 딜레이 최소값

    [SerializeField]
    float reactionDelayMax;  // 반응 딜레이 최대값

    [SerializeField]
    float reactionDelayDistance;  // 반응 딜레이를 결정하는 거리

    // 필드 변수들 선언
    Target selfTarget;  // 자기 자신을 향한 타겟
    Plane targetPlane;  // 현재 타겟으로 지정된 비행체(비행기)
    Vector3 lastInput;  // 마지막 입력값 저장
    bool isRecoveringSpeed;  // 속도 회복 중인지 여부

    float missileDelayTimer;  // 미사일 발사 딜레이 타이머
    float missileCooldownTimer;  // 미사일 발사 쿨다운 타이머

    bool cannonFiring;  // 대포 발사 중인지 여부
    float cannonBurstTimer;  // 대포 연사 타이머
    float cannonCooldownTimer;  // 대포 발사 쿨다운 타이머

    struct ControlInput
    {
        public float time;  // 입력 시간
        public Vector3 targetPosition;  // 목표 위치
    }

    Queue<ControlInput> inputQueue;  // 입력값을 저장하는 큐

    bool dodging;  // 회피 중인지 여부
    Vector3 lastDodgePoint;  // 마지막 회피 지점
    List<Vector3> dodgeOffsets;  // 회피 지점들의 리스트
    const float dodgeUpdateInterval = 0.25f;  // 회피 지점 업데이트 간격
    float dodgeTimer;  // 회피 타이머

    void Start()
    {
        // 초기화 메서드
        selfTarget = plane.GetComponent<Target>();  // 자신을 타겟으로 설정

        if (plane.Target != null)
        {
            targetPlane = plane.Target.GetComponent<Plane>();  // 현재 타겟의 비행체로 설정
        }

        dodgeOffsets = new List<Vector3>();
        inputQueue = new Queue<ControlInput>();
    }

    Vector3 AvoidGround()
    {
        // 지면 회피 로직
        var roll = plane.Rigidbody.rotation.eulerAngles.z;
        if (roll > 180f) roll -= 360f;
        return new Vector3(-1, 0, Mathf.Clamp(-roll * rollFactor, -1, 1));
    }

    Vector3 RecoverSpeed()
    {
        // 속도 회복 로직
        var roll = plane.Rigidbody.rotation.eulerAngles.z;
        var pitch = plane.Rigidbody.rotation.eulerAngles.x;
        if (roll > 180f) roll -= 360f;
        if (pitch > 180f) pitch -= 360f;
        return new Vector3(Mathf.Clamp(-pitch, -1, 1), 0, Mathf.Clamp(-roll * rollFactor, -1, 1));
    }

    Vector3 GetTargetPosition()
    {
        // 타겟의 위치 계산
        if (plane.Target == null)
        {
            return plane.Rigidbody.position;
        }

        var targetPosition = plane.Target.Position;

        if (Vector3.Distance(targetPosition, plane.Rigidbody.position) < cannonRange)
        {
            // 대포 사거리 내에서는 타겟의 위치를 예측하여 반환
            return Utilities.FirstOrderIntercept(plane.Rigidbody.position, plane.Rigidbody.velocity, bulletSpeed, targetPosition, plane.Target.Velocity);
        }

        return targetPosition;
    }

    Vector3 CalculateSteering(float dt, Vector3 targetPosition) {
        if (plane.Target != null && targetPlane.Health == 0) {
            return new Vector3();
        }

        var error = targetPosition - plane.Rigidbody.position;
        error = Quaternion.Inverse(plane.Rigidbody.rotation) * error;   //transform into local space

        var errorDir = error.normalized;
        var pitchError = new Vector3(0, error.y, error.z).normalized;
        var rollError = new Vector3(error.x, error.y, 0).normalized;
        var yawError = new Vector3(error.x, 0, error.z).normalized;

        var targetInput = new Vector3();

        var pitch = Vector3.SignedAngle(Vector3.forward, pitchError, Vector3.right);
        if (-pitch < pitchUpThreshold) pitch += 360f;
        targetInput.x = pitch;

        if (Vector3.Angle(Vector3.forward, errorDir) < fineSteeringAngle) {
            var yaw = Vector3.SignedAngle(Vector3.forward, yawError, Vector3.up);
            targetInput.y = yaw * yawFactor;
        } else {
            var roll = Vector3.SignedAngle(Vector3.up, rollError, Vector3.forward);
            targetInput.z = roll * rollFactor;
        }

        targetInput.x = Mathf.Clamp(targetInput.x, -1, 1);
        targetInput.y = Mathf.Clamp(targetInput.y, -1, 1);
        targetInput.z = Mathf.Clamp(targetInput.z, -1, 1);

        var input = Vector3.MoveTowards(lastInput, targetInput, steeringSpeed * dt);
        lastInput = input;

        return input;
    }

    Vector3 GetMissileDodgePosition(float dt, Missile missile) {
        dodgeTimer = Mathf.Max(0, dodgeTimer - dt);
        var missilePos = missile.Rigidbody.position;

        var dist = Mathf.Max(minMissileDodgeDistance, Vector3.Distance(missilePos, plane.Rigidbody.position));

        //calculate dodge points
        if (dodgeTimer == 0) {
            var missileForward = missile.Rigidbody.rotation * Vector3.forward;
            dodgeOffsets.Clear();

            //4 dodge points: up, down, left, right

            dodgeOffsets.Add(new Vector3(0, dist, 0));
            dodgeOffsets.Add(new Vector3(0, -dist, 0));
            dodgeOffsets.Add(Vector3.Cross(missileForward, Vector3.up) * dist);
            dodgeOffsets.Add(Vector3.Cross(missileForward, Vector3.up) * -dist);

            dodgeTimer = dodgeUpdateInterval;
        }

        //select nearest dodge point
        float min = float.PositiveInfinity;
        Vector3 minDodge = missilePos + dodgeOffsets[0];

        foreach (var offset in dodgeOffsets) {
            var dodgePosition = missilePos + offset;
            var offsetDist = Vector3.Distance(dodgePosition, lastDodgePoint);

            if (offsetDist < min) {
                minDodge = dodgePosition;
                min = offsetDist;
            }
        }

        lastDodgePoint = minDodge;
        return minDodge;
    }

    float CalculateThrottle(float minSpeed, float maxSpeed) {
        float input = 0;

        if (plane.LocalVelocity.z < minSpeed) {
            input = 1;
        } else if (plane.LocalVelocity.z > maxSpeed) {
            input = -1;
        }

        return input;
    }

    void CalculateWeapons(float dt) {
        if (plane.Target == null) return;

        if (canUseMissiles) {
            CalculateMissiles(dt);
        }

        if (canUseCannon) {
            CalculateCannon(dt);
        }
    }

    void CalculateMissiles(float dt) {
        missileDelayTimer = Mathf.Max(0, missileDelayTimer - dt);
        missileCooldownTimer = Mathf.Max(0, missileCooldownTimer - dt);

        var error = plane.Target.Position - plane.Rigidbody.position;
        var range = error.magnitude;
        var targetDir = error.normalized;
        var targetAngle = Vector3.Angle(targetDir, plane.Rigidbody.rotation * Vector3.forward);

        if (!plane.MissileLocked || !(targetAngle < missileMaxFireAngle || (180f - targetAngle) < missileMaxFireAngle)) {
            //don't fire if not locked or target is too off angle
            //can fire if angle is close to 0 (chasing) or 180 (head on)
            missileDelayTimer = missileLockFiringDelay;
        }

        if (range < missileMaxRange && range > missileMinRange && missileDelayTimer == 0 && missileCooldownTimer == 0) {
            plane.TryFireMissile();
            missileCooldownTimer = missileFiringCooldown;
        }
    }

    void CalculateCannon(float dt) {
        if (targetPlane.Health == 0) {
            cannonFiring = false;
            return;
        }

        if (cannonFiring) {
            cannonBurstTimer = Mathf.Max(0, cannonBurstTimer - dt);

            if (cannonBurstTimer == 0) {
                cannonFiring = false;
                cannonCooldownTimer = cannonBurstCooldown;
                plane.SetCannonInput(false);
            }
        } else {
            cannonCooldownTimer = Mathf.Max(0, cannonCooldownTimer - dt);

            var targetPosition = Utilities.FirstOrderIntercept(plane.Rigidbody.position, plane.Rigidbody.velocity, bulletSpeed, plane.Target.Position, plane.Target.Velocity);

            var error = targetPosition - plane.Rigidbody.position;
            var range = error.magnitude;
            var targetDir = error.normalized;
            var targetAngle = Vector3.Angle(targetDir, plane.Rigidbody.rotation * Vector3.forward);

            if (range < cannonRange && targetAngle < cannonMaxFireAngle && cannonCooldownTimer == 0) {
                cannonFiring = true;
                cannonBurstTimer = cannonBurstLength;
                plane.SetCannonInput(true);
            }
        }
    }

    void SteerToTarget(float dt, Vector3 planePosition) {
        bool foundTarget = false;
        Vector3 steering = Vector3.zero;
        Vector3 targetPosition = Vector3.zero;

        var delay = reactionDelayMax;

        if (Vector3.Distance(planePosition, plane.Rigidbody.position) < reactionDelayDistance) {
            delay = reactionDelayMin;
        }

        while (inputQueue.Count > 0) {
            var input = inputQueue.Peek();

            if (input.time + delay <= Time.time) {
                targetPosition = input.targetPosition;
                inputQueue.Dequeue();
                foundTarget = true;
            } else {
                break;
            }
        }

        if (foundTarget) {
            steering = CalculateSteering(dt, targetPosition);
        }

        plane.SetControlInput(steering);
    }

    void FixedUpdate() {
        if (plane.Dead) return;
        var dt = Time.fixedDeltaTime;

        Vector3 steering = Vector3.zero;
        float throttle;
        bool emergency = false;
        Vector3 targetPosition = plane.Target.Position;

        var velocityRot = Quaternion.LookRotation(plane.Rigidbody.velocity.normalized);
        var ray = new Ray(plane.Rigidbody.position, velocityRot * Quaternion.Euler(groundAvoidanceAngle, 0, 0) * Vector3.forward);

        if (Physics.Raycast(ray, groundCollisionDistance + plane.LocalVelocity.z, groundCollisionMask.value)) {
            steering = AvoidGround();
            throttle = CalculateThrottle(groundAvoidanceMinSpeed, groundAvoidanceMaxSpeed);
            emergency = true;
        } else {
            var incomingMissile = selfTarget.GetIncomingMissile();
            if (incomingMissile != null) {
                if (dodging == false) {
                    //start dodging
                    dodging = true;
                    lastDodgePoint = plane.Rigidbody.position;
                    dodgeTimer = 0;
                }

                var dodgePosition = GetMissileDodgePosition(dt, incomingMissile);
                steering = CalculateSteering(dt, dodgePosition);
                emergency = true;
            } else {
                dodging = false;
                targetPosition = GetTargetPosition();
            }

            if (incomingMissile == null && (plane.LocalVelocity.z < recoverSpeedMin || isRecoveringSpeed)) {
                isRecoveringSpeed = plane.LocalVelocity.z < recoverSpeedMax;

                steering = RecoverSpeed();
                throttle = 1;
                emergency = true;
            } else {
                throttle = CalculateThrottle(minSpeed, maxSpeed);
            }
        }

        inputQueue.Enqueue(new ControlInput {
            time = Time.time,
            targetPosition = targetPosition,
        });

        plane.SetThrottleInput(throttle);

        if (emergency) {
            if (isRecoveringSpeed) {
                //reduce steering strength while recovering speed
                steering.x = Mathf.Clamp(steering.x, -0.5f, 0.5f);
            }

            plane.SetControlInput(steering);
        } else {
            SteerToTarget(dt, plane.Target.Position);
        }

        CalculateWeapons(dt);
    }
}
