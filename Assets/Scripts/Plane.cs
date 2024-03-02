using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEditor;
using Google.Protobuf;

using System.Reflection;
using Protocol;

public class Plane : MonoBehaviour {
    [SerializeField]
    float maxHealth;
    [SerializeField]
    float health;
    [SerializeField]
    float maxThrust;
    [SerializeField]
    float throttleSpeed;
    [SerializeField]
    float gLimit;
    [SerializeField]
    float gLimitPitch;

    [Header("Lift")]
    [SerializeField]
    float liftPower;
    [SerializeField]
    AnimationCurve liftAOACurve;
    [SerializeField]
    float inducedDrag;
    [SerializeField]
    AnimationCurve inducedDragCurve;
    [SerializeField]
    float rudderPower;
    [SerializeField]
    AnimationCurve rudderAOACurve;
    [SerializeField]
    AnimationCurve rudderInducedDragCurve;
    [SerializeField]
    float flapsLiftPower;
    [SerializeField]
    float flapsAOABias;
    [SerializeField]
    float flapsDrag;
    [SerializeField]
    float flapsRetractSpeed;

    [Header("Steering")]
    [SerializeField]
    Vector3 turnSpeed;
    [SerializeField]
    Vector3 turnAcceleration;
    [SerializeField]
    AnimationCurve steeringCurve;

    [Header("Drag")]
    [SerializeField]
    AnimationCurve dragForward;
    [SerializeField]
    AnimationCurve dragBack;
    [SerializeField]
    AnimationCurve dragLeft;
    [SerializeField]
    AnimationCurve dragRight;
    [SerializeField]
    AnimationCurve dragTop;
    [SerializeField]
    AnimationCurve dragBottom;
    [SerializeField]
    Vector3 angularDrag;
    [SerializeField]
    float airbrakeDrag;

    [Header("Misc")]
    [SerializeField]
    List<Collider> landingGear;
    [SerializeField]
    PhysicMaterial landingGearBrakesMaterial;
    [SerializeField]
    List<GameObject> graphics;
    [SerializeField]
    GameObject damageEffect;
    [SerializeField]
    GameObject deathEffect;
    [SerializeField]
    bool flapsDeployed;
    [SerializeField]
    float initialSpeed;

    [Header("Weapons")]
    [SerializeField]
    List<Transform> hardpoints;
    [SerializeField]
    float missileReloadTime;
    [SerializeField]
    float missileDebounceTime;
    [SerializeField]
    GameObject missilePrefab;
    [SerializeField]
    Target target;
    [SerializeField]
    float lockRange;
    [SerializeField]
    float lockSpeed;
    [SerializeField]
    float lockAngle;
    [SerializeField]
    [Tooltip("Firing rate in Rounds Per Minute")]
    float cannonFireRate;
    [SerializeField]
    float cannonDebounceTime;
    [SerializeField]
    float cannonSpread;
    [SerializeField]
    Transform cannonSpawnPoint;
    [SerializeField]
    GameObject bulletPrefab;

    new PlaneAnimation animation;

    float throttleInput;
    Vector3 controlInput;

    Vector3 lastVelocity;
    PhysicMaterial landingGearDefaultMaterial;

    int missileIndex;
    List<float> missileReloadTimers;
    float missileDebounceTimer;
    Vector3 missileLockDirection;

    bool cannonFiring;
    float cannonDebounceTimer;
    float cannonFiringTimer;

    private Socket clientSocket;
    private byte[] receiveBuffer = new byte[1024];
    private bool serverConnected = false;

    private PacketManager packetManager;


    // 메시지 타입에 대한 딕셔너리 생성
    private static Dictionary<string, MethodInfo> messageMethods = new Dictionary<string, MethodInfo>();

    //
    private ulong playerId;

    public float MaxHealth {
        get {
            return maxHealth;
        }
        set {
            maxHealth = Mathf.Max(0, value);
        }
    }

    public float Health {
        get {
            return health;
        }
        private set {
            health = Mathf.Clamp(value, 0, maxHealth);

            if (health <= MaxHealth * .5f && health > 0) {
                damageEffect.SetActive(true);
            } else {
                damageEffect.SetActive(false);
            }

            if (health == 0 && MaxHealth != 0 && !Dead) {
                Die();
            }
        }
    }

    public bool Dead { get; private set; }

    public Rigidbody Rigidbody { get; private set; }
    public float Throttle { get; private set; }
    public Vector3 EffectiveInput { get; private set; }
    public Vector3 Velocity { get; private set; }
    public Vector3 LocalVelocity { get; private set; }
    public Vector3 LocalGForce { get; private set; }
    public Vector3 LocalAngularVelocity { get; private set; }
    public float AngleOfAttack { get; private set; }
    public float AngleOfAttackYaw { get; private set; }
    public bool AirbrakeDeployed { get; private set; }

    public bool FlapsDeployed {
        get {
            return flapsDeployed;
        }
        private set {
            flapsDeployed = value;

            foreach (var lg in landingGear) {
                lg.enabled = value;
            }
        }
    }

    public bool MissileLocked { get; private set; }
    public bool MissileTracking { get; private set; }
    public Target Target {
        get {
            return target;
        }
    }
    public Vector3 MissileLockDirection {
        get {
            return Rigidbody.rotation * missileLockDirection;
        }
    }

    private static Plane instance;
    public static Plane Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Plane>();
            }
            return instance;
        }
    }


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            packetManager = gameObject.AddComponent<PacketManager>();
            IOCPServerConnect();
        }
        else
        {
            // Destroy(gameObject); // 다른 오브젝트는 파괴
        }

    }

 

    void Start() {
       
        animation = GetComponent<PlaneAnimation>();
        Rigidbody = GetComponent<Rigidbody>();

        if (landingGear.Count > 0) {
            landingGearDefaultMaterial = landingGear[0].sharedMaterial;
        }

        missileReloadTimers = new List<float>(hardpoints.Count);

        foreach (var h in hardpoints) {
            missileReloadTimers.Add(0);
        }

        missileLockDirection = Vector3.forward;

        Rigidbody.velocity = Rigidbody.rotation * new Vector3(0, 0, initialSpeed);

  
    }

    void SpawnF15()
    {
        // 프리팹 경로 설정
        string prefabPath = "Assets/Prefabs/F15.prefab";

        // 프리팹 로드
        GameObject f15Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (f15Prefab != null)
        {
            // 프리팹 복제
            GameObject newF15 = Instantiate(f15Prefab);

            // 복제된 오브젝트 이름 변경
            newF15.name = "F15(2)";

            // 새로운 위치 및 회전으로 이동
            newF15.transform.position = new Vector3(UnityEngine.Random.Range(-2300f, -2500f), UnityEngine.Random.Range(0f, 150f), UnityEngine.Random.Range(-2300f, -2500f));
            newF15.transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
        }
        else
        {
            Debug.LogError("F15Prefab을 로드할 수 없습니다.");
        }
    }

    public void SetThrottleInput(float input) {
        if (Dead) return;
        throttleInput = input;
    }

    public void SetControlInput(Vector3 input) {
        if (Dead) return;
        controlInput = Vector3.ClampMagnitude(input, 1);
    }

    public void SetCannonInput(bool input) {
        if (Dead) return;
        cannonFiring = input;
    }

    public void ToggleFlaps() {
        if (LocalVelocity.z < flapsRetractSpeed) {
            FlapsDeployed = !FlapsDeployed;
        }
    }

    public void ApplyDamage(float damage) {
        Health -= damage;
    }

    void Die() {
        throttleInput = 0;
        Throttle = 0;
        Dead = true;
        cannonFiring = false;

        damageEffect.GetComponent<ParticleSystem>().Pause();
        deathEffect.SetActive(true);
    }

    void UpdateThrottle(float dt) // 게임패드 동작코드
    {
        float target = 0;
        if (throttleInput > 0) target = 1;

        // 스로틀 입력 범위: [-1, 1]
        // 스로틀 값 범위: [0, 1]
        // Throttle 값은 Utilities.MoveTo 함수를 사용하여 목표값(target)으로 이동
        // 이동 속도는 throttleSpeed와 현재 입력의 절댓값에 비례
        Throttle = Utilities.MoveTo(Throttle, target, throttleSpeed * Mathf.Abs(throttleInput), dt);

        // 에어브레이크가 활성화된 경우
        AirbrakeDeployed = Throttle == 0 && throttleInput == -1;

        // 에어브레이크가 활성화되면 착륙장치의 재질을 변경하여 브레이크 효과
        if (AirbrakeDeployed)
        {
            foreach (var lg in landingGear)
            {
                lg.sharedMaterial = landingGearBrakesMaterial;
            }
        }
        else
        {
            // 에어브레이크가 비활성화된 경우 원래의 착륙장치 재질로 변경
            foreach (var lg in landingGear)
            {
                lg.sharedMaterial = landingGearDefaultMaterial;
            }
        }
    }

    void UpdateFlaps() {
        if (LocalVelocity.z > flapsRetractSpeed) {
            FlapsDeployed = false;
        }
    }

    void CalculateAngleOfAttack() {
        if (LocalVelocity.sqrMagnitude < 0.1f) { // 비행속도가 없다면 종료
            AngleOfAttack = 0;
            AngleOfAttackYaw = 0;
            return;
        }

        AngleOfAttack = Mathf.Atan2(-LocalVelocity.y, LocalVelocity.z); // 비행체의 상하 움직임 공격각도 계산
        AngleOfAttackYaw = Mathf.Atan2(LocalVelocity.x, LocalVelocity.z); //비행체의 좌우 움직임 공격각도 계산
    }

    void CalculateGForce(float dt) {
        var invRotation = Quaternion.Inverse(Rigidbody.rotation); // 회전의 역값 계산
        var acceleration = (Velocity - lastVelocity) / dt; // 가속도를 계산
        LocalGForce = invRotation * acceleration;
        lastVelocity = Velocity;
    }

    void CalculateState(float dt) {
        var invRotation = Quaternion.Inverse(Rigidbody.rotation); // 회전의 역값을 계산
        Velocity = Rigidbody.velocity;
        LocalVelocity = invRotation * Velocity;  // 변환된 속도를 계산
        LocalAngularVelocity = invRotation * Rigidbody.angularVelocity;  //변환된 가속도를 계산

        CalculateAngleOfAttack();
    }

    void UpdateThrust() {
        Rigidbody.AddRelativeForce(Throttle * maxThrust * Vector3.forward);
    }

    void UpdateDrag() {
        var lv = LocalVelocity;
        var lv2 = lv.sqrMagnitude;  //velocity squared

        float airbrakeDrag = AirbrakeDeployed ? this.airbrakeDrag : 0;
        float flapsDrag = FlapsDeployed ? this.flapsDrag : 0;

        //calculate coefficient of drag depending on direction on velocity
        var coefficient = Utilities.Scale6(
            lv.normalized,
            dragRight.Evaluate(Mathf.Abs(lv.x)), dragLeft.Evaluate(Mathf.Abs(lv.x)),
            dragTop.Evaluate(Mathf.Abs(lv.y)), dragBottom.Evaluate(Mathf.Abs(lv.y)),
            dragForward.Evaluate(Mathf.Abs(lv.z)) + airbrakeDrag + flapsDrag,   //include extra drag for forward coefficient
            dragBack.Evaluate(Mathf.Abs(lv.z))
        );

        var drag = coefficient.magnitude * lv2 * -lv.normalized;    //drag is opposite direction of velocity

        Rigidbody.AddRelativeForce(drag);
    }

    Vector3 CalculateLift(float angleOfAttack, Vector3 rightAxis, float liftPower, AnimationCurve aoaCurve, AnimationCurve inducedDragCurve) {
        var liftVelocity = Vector3.ProjectOnPlane(LocalVelocity, rightAxis);    //project velocity onto YZ plane
        var v2 = liftVelocity.sqrMagnitude;                                     //square of velocity

        //lift = velocity^2 * coefficient * liftPower
        //coefficient varies with AOA
        var liftCoefficient = aoaCurve.Evaluate(angleOfAttack * Mathf.Rad2Deg);
        var liftForce = v2 * liftCoefficient * liftPower;

        //lift is perpendicular to velocity
        var liftDirection = Vector3.Cross(liftVelocity.normalized, rightAxis);
        var lift = liftDirection * liftForce;

        //induced drag varies with square of lift coefficient
        var dragForce = liftCoefficient * liftCoefficient;
        var dragDirection = -liftVelocity.normalized;
        var inducedDrag = dragDirection * v2 * dragForce * this.inducedDrag * inducedDragCurve.Evaluate(Mathf.Max(0, LocalVelocity.z));

        return lift + inducedDrag;
    }

    void UpdateLift() {
        if (LocalVelocity.sqrMagnitude < 1f) return;

        float flapsLiftPower = FlapsDeployed ? this.flapsLiftPower : 0;
        float flapsAOABias = FlapsDeployed ? this.flapsAOABias : 0;

        var liftForce = CalculateLift(
            AngleOfAttack + (flapsAOABias * Mathf.Deg2Rad), Vector3.right,
            liftPower + flapsLiftPower,
            liftAOACurve,
            inducedDragCurve
        );

        var yawForce = CalculateLift(AngleOfAttackYaw, Vector3.up, rudderPower, rudderAOACurve, rudderInducedDragCurve);

        Rigidbody.AddRelativeForce(liftForce);
        Rigidbody.AddRelativeForce(yawForce);
    }

    void UpdateAngularDrag() {
        var av = LocalAngularVelocity;
        var drag = av.sqrMagnitude * -av.normalized;    //squared, opposite direction of angular velocity
        Rigidbody.AddRelativeTorque(Vector3.Scale(drag, angularDrag), ForceMode.Acceleration);  //ignore rigidbody mass
    }

    Vector3 CalculateGForce(Vector3 angularVelocity, Vector3 velocity) {
        //estiamte G Force from angular velocity and velocity
        //Velocity = AngularVelocity * Radius
        //G = Velocity^2 / R
        //G = (Velocity * AngularVelocity * Radius) / Radius
        //G = Velocity * AngularVelocity
        //G = V cross A
        return Vector3.Cross(angularVelocity, velocity);
    }

    Vector3 CalculateGForceLimit(Vector3 input) {
        return Utilities.Scale6(input,
            gLimit, gLimitPitch,    //pitch down, pitch up
            gLimit, gLimit,         //yaw
            gLimit, gLimit          //roll
        ) * 9.81f;
    }

    float CalculateGLimiter(Vector3 controlInput, Vector3 maxAngularVelocity) {
        if (controlInput.magnitude < 0.01f) {
            return 1;
        }

        //if the player gives input with magnitude less than 1, scale up their input so that magnitude == 1
        var maxInput = controlInput.normalized;

        var limit = CalculateGForceLimit(maxInput);
        var maxGForce = CalculateGForce(Vector3.Scale(maxInput, maxAngularVelocity), LocalVelocity);

        if (maxGForce.magnitude > limit.magnitude) {
            //example:
            //maxGForce = 16G, limit = 8G
            //so this is 8 / 16 or 0.5
            return limit.magnitude / maxGForce.magnitude;
        }

        return 1;
    }

    float CalculateSteering(float dt, float angularVelocity, float targetVelocity, float acceleration) {
        var error = targetVelocity - angularVelocity;
        var accel = acceleration * dt;
        return Mathf.Clamp(error, -accel, accel);
    }

    void UpdateSteering(float dt) {
        var speed = Mathf.Max(0, LocalVelocity.z);
        var steeringPower = steeringCurve.Evaluate(speed);

        var gForceScaling = CalculateGLimiter(controlInput, turnSpeed * Mathf.Deg2Rad * steeringPower);

        var targetAV = Vector3.Scale(controlInput, turnSpeed * steeringPower * gForceScaling);
        var av = LocalAngularVelocity * Mathf.Rad2Deg;

        var correction = new Vector3(
            CalculateSteering(dt, av.x, targetAV.x, turnAcceleration.x * steeringPower),
            CalculateSteering(dt, av.y, targetAV.y, turnAcceleration.y * steeringPower),
            CalculateSteering(dt, av.z, targetAV.z, turnAcceleration.z * steeringPower)
        );

        Rigidbody.AddRelativeTorque(correction * Mathf.Deg2Rad, ForceMode.VelocityChange);    //ignore rigidbody mass

        var correctionInput = new Vector3(
            Mathf.Clamp((targetAV.x - av.x) / turnAcceleration.x, -1, 1),
            Mathf.Clamp((targetAV.y - av.y) / turnAcceleration.y, -1, 1),
            Mathf.Clamp((targetAV.z - av.z) / turnAcceleration.z, -1, 1)
        );

        var effectiveInput = (correctionInput + controlInput) * gForceScaling;

        EffectiveInput = new Vector3(
            Mathf.Clamp(effectiveInput.x, -1, 1),
            Mathf.Clamp(effectiveInput.y, -1, 1),
            Mathf.Clamp(effectiveInput.z, -1, 1)
        );
    }

    public void TryFireMissile() {
        if (Dead) return;

        //try all available missiles
        for (int i = 0; i < hardpoints.Count; i++) {
            var index = (missileIndex + i) % hardpoints.Count;
            if (missileDebounceTimer == 0 && missileReloadTimers[index] == 0) {
                FireMissile(index);

                missileIndex = (index + 1) % hardpoints.Count;
                missileReloadTimers[index] = missileReloadTime;
                missileDebounceTimer = missileDebounceTime;

                animation.ShowMissileGraphic(index, false);
                break;
            }
        }
    }

    void FireMissile(int index) {
        var hardpoint = hardpoints[index];
        var missileGO = Instantiate(missilePrefab, hardpoint.position, hardpoint.rotation);
        var missile = missileGO.GetComponent<Missile>();
        missile.Launch(this, MissileLocked ? Target : null);
        SpawnF15();

    }

    void UpdateWeapons(float dt) {
        UpdateWeaponCooldown(dt);
        UpdateMissileLock(dt);
        UpdateCannon(dt);
    }

    void UpdateWeaponCooldown(float dt) {
        missileDebounceTimer = Mathf.Max(0, missileDebounceTimer - dt);
        cannonDebounceTimer = Mathf.Max(0, cannonDebounceTimer - dt);
        cannonFiringTimer = Mathf.Max(0, cannonFiringTimer - dt);

        for (int i = 0; i < missileReloadTimers.Count; i++) {
            missileReloadTimers[i] = Mathf.Max(0, missileReloadTimers[i] - dt);

            if (missileReloadTimers[i] == 0) {
                animation.ShowMissileGraphic(i, true);
            }
        }
    }

    void UpdateMissileLock(float dt) {
        //default neutral position is forward
        Vector3 targetDir = Vector3.forward;
        MissileTracking = false;

        if (Target != null && !Target.Plane.Dead) {
            var error = target.Position - Rigidbody.position;
            var errorDir = Quaternion.Inverse(Rigidbody.rotation) * error.normalized; //transform into local space

            if (error.magnitude <= lockRange && Vector3.Angle(Vector3.forward, errorDir) <= lockAngle) {
                MissileTracking = true;
                targetDir = errorDir;
            }
        }

        //missile lock either rotates towards the target, or towards the neutral position
        missileLockDirection = Vector3.RotateTowards(missileLockDirection, targetDir, Mathf.Deg2Rad * lockSpeed * dt, 0);

        MissileLocked = Target != null && MissileTracking && Vector3.Angle(missileLockDirection, targetDir) < lockSpeed * dt;
    }

    void UpdateCannon(float dt) {
        if (cannonFiring && cannonFiringTimer == 0) {
            cannonFiringTimer = 60f / cannonFireRate;

            var spread = UnityEngine.Random.insideUnitCircle * cannonSpread;

            var bulletGO = Instantiate(bulletPrefab, cannonSpawnPoint.position, cannonSpawnPoint.rotation * Quaternion.Euler(spread.x, spread.y, 0));
            var bullet = bulletGO.GetComponent<Bullet>();
            bullet.Fire(this);
        }
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

      
        CalculateState(dt); 
        CalculateGForce(dt); 
        UpdateFlaps(); // 플랩의 상태 업데이트

        // 사용자 입력을 처리
        //HandleMouseAndKeyboardInput();
        UpdateThrottle(dt);

        if (!Dead)
        {
            // 업데이트 적용
            UpdateThrust();
            UpdateLift();
            UpdateSteering(dt);
        }
        else
        {
            // 속도에 맞춰 정렬
            Vector3 up = Rigidbody.rotation * Vector3.up;
            Vector3 forward = Rigidbody.velocity.normalized;
            Rigidbody.rotation = Quaternion.LookRotation(forward, up);
        }

        UpdateDrag();
        UpdateAngularDrag();

        // 다시 계산하여 다른 시스템에서 비행기 상태를 읽을 수 있게 함
        CalculateState(dt);

        // 무기 상태 업데이트
        UpdateWeapons(dt);
        //Debug.Log("transform.position"+  transform.position);
        // EnumMessage 클래스 사용 예시


        //var loginMessage = new C_CHAT() { 
        // Msg = "?"
        //};
        //SendToServer(loginMessage);


    }



    void OnCollisionEnter(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            var contact = collision.contacts[i];

            if (landingGear.Contains(contact.thisCollider))
            {
                return;
            }

            Health = 0;

            Rigidbody.isKinematic = true;
            Rigidbody.position = contact.point;
            Rigidbody.rotation = Quaternion.Euler(0, Rigidbody.rotation.eulerAngles.y, 0);

            foreach (var go in graphics)
            {
                go.SetActive(false);
            }

            return;
        }
    }



    private void IOCPServerConnect()
    {
        if (serverConnected)
        {
            return; // 이미 연결되었으면 중복 호출 방지
        }

        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress serverIP = IPAddress.Parse("127.0.0.1");
        IPEndPoint serverEndPoint = new IPEndPoint(serverIP, 7777);

        try
        {
            clientSocket.BeginConnect(serverEndPoint, ConnectCallback, null);
            serverConnected = true; // 연결 후에 상태 업데이트
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to server: {e.Message}");
        }
    }

    private void ConnectCallback(IAsyncResult result)
    {
        try
        {
            clientSocket.EndConnect(result);
            Debug.Log("Connected to server");
            if (serverConnected)
            {
                // 로그인 요청 보내기

                var loginMessage = new C_LOGIN();
                var enterMessage = new C_ENTER_GAME()
                {
                    PlayerIndex = 0
                };
                SendToServer(loginMessage, PacketType.PKT_C_LOGIN);
                SendToServer(enterMessage, PacketType.PKT_C_ENTER_GAME);
                StartReceive();
            }
        
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to server: {e.Message}");
        }
    }

    private void StartReceive()
    {
        // 이미 서버에 연결되어 있는 경우 추가적인 연결 시도를 하지 않음
        if (!serverConnected)
        {
            Debug.LogWarning("Cannot start receiving data. Server is not connected.");
            return;
        }

        byte[] buffer = new byte[1024]; // 적절한 버퍼 크기를 정의해야 합니다.
        clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, buffer);
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        try
        {
            int bytesRead = clientSocket.EndReceive(result);

            if (bytesRead > 0)
            {
                byte[] receivedData = (byte[])result.AsyncState;

                // 서버로부터 받은 데이터를 처리
                packetManager.OnRecv(receivedData, bytesRead);

                // 추가적인 데이터 수신을 위해 다시 BeginReceive 호출
                clientSocket.BeginReceive(receivedData, 0, receivedData.Length, SocketFlags.None, ReceiveCallback, receivedData);
            }
            else
            {
                // 연결이 끊겼거나 데이터가 없는 경우 처리할 작업을 여기에 추가
                Debug.Log("Connection closed or no data received from server.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to receive data: {e.Message}");
        }
    }




    private void SendToServer(IMessage message, PacketType type)
    {
        // 메시지를 직렬화하여 패킷에 헤더를 추가하고 서버로 전송
        byte[] serializedData = packetManager.SerializeWithHeader(message, type);
        SendMessage(serializedData);
    }

    private void SendMessage(byte[] data)
    {
        try
        {
            // 비동기 방식으로 데이터를 보낸다.
            clientSocket.BeginSend(data, 0, data.Length, SocketFlags.None, SendCallback, null);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send data to server: {e.Message}");
        }
    }

    private void SendCallback(IAsyncResult result)
    {
        try
        {
            // 비동기 작업을 완료하고 전송된 바이트 수를 반환
            int sentBytes = clientSocket.EndSend(result);
            Debug.Log($"Sent {sentBytes} bytes to server");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send data to server: {e.Message}");
        }
    }



    void OnDestroy()
    {
        if (clientSocket != null && clientSocket.Connected)
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
    }

   





}