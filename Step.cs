/*
1. 3D 쿼터뷰 액션 게임 - 플레이어 이동

    #1. 지형 만들기
        [a]. 3D 오브젝트에서 cube 생성
            GenerateLitening
            scale 100 / 1 / 100
            Floor로 명명
        [b]. Cube를 추가로 생성하여 Wall로 명명
            110 / 10 / 10
            테두리에 위치 지정
            카메라에 가까운 두개의 벽의 MeshRenderer를 비활성화
        [c]. Materials 폴더 생성
            Material 생성 Floor로 명명
                Albedo _Tile로 지정
            Floor, Wall 오브젝트에 Material 지정
            Material의 Tileing 조절
            색 지정

    #2. 플레이어 만들기
        [a]. 플레이어 프리팹을 하이어라키 창에 등록
        [b]. 리지드바디 + 캡슐 콜라이더 장착
        [c]. 스크립트 Player를 만들고 장착
        [d]. Player 객체 y축 지정
            캡슐 콜라이더 크기 조절

    #3. 기본 이동 구현
        [a]. Player 스크립트에서 Transform 이동 로직을 작성한다.
        [b]. 속성으로 수평, 수직 값을 받고 움직이는 방향을 받는다.
            float hAxis; float vAxis; Vector3 moveVec;
        [c]. Update함수에서 수평, 수직 값을 입력 받아 속성에 저장한다.
            방향 속성에 Vector3로 x, z축 값을 저장하여 방향을 지정한다.
                이때 크기를 정규화 시켜 준다.
                moveVec = new Vector3(hAxis, 0, vAxis).normalized;
        [d]. 현재 위치에 이동할 방향 값 * 속도 * 시간
            속도를 public 속성으로 갖는다.
        [e]. Player의 리지드바디에서 Rotation x, z 축을 얼린다.
            Collision Detection의 값을 Continuous로 바꾸어 물리적 충돌 처리를 더 자주하도록 한다.

    #4. 애니메이션
        [a]. 애니메이션 폴더를 만들고 애니메이터 컨트롤러 만들기 Player
            Player 객체의 자식 MeshObject에 애니메이터 컨트롤러를 컴포넌트로 넣어 준다.
        [b]. Model 폴더에서 플레이어 애니메이션을 애니메이터에 드래그 드랍 한다.
            Idle, Walk, Run
            트랜지션으로 서로 연결
            파라미터 bool isWalk, isRun 생성
            트랜지션의 컨디션으로 연결
        [c]. 기본 이동을 Run으로 지정하고 shift 키를 눌렀을 때 걷기로 한다.
        [d]. Player 스크립트로 가서 애니메이터를 속성으로 갖는다.
            Awake() 함수에서 자식으로 부터 컴포넌트를 받는다.
        [e]. Update함수에서 플레이어가 움직일 때 SetBool로 방향 Vector값이 0인지를 전달한다.
            anim.SetBool("isRun", moveVec != Vector3.zero);
        [f]. InputManager에서 Shift키를 추가한다.
            Walk 이름으로 left shift로 설정
        [g]. bool 타입의 속성 걷기를 받는다. wDown
        [h]. Update() 함수에서 wDown에 shift 값을 받는다.
            파라미터로 wDonw을 전달한다.
        [i]. 삼항 연산자로 걷기를 하였을 때 속도를 낮춘다.
            transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

    #5. 기본 회전 구현
        [a]. LookAt() 함수를 통해 지정된 벡터를 향해서 회전시켜준다.
            transform.LookAt(transform.position += moveVec);

    #6. 카메라 이동
        [a]. 카메라의 위치를 이동, 0 / 21 / -11 방향 60 / 0 / 0
        [b]. follow 스크립트를 만들고 메인 카메라 컴포넌트로 부착
        [c]. 카메라가 따라가야할 목표의 위치와 목표와의 거리를 속성으로 갖는다.
            public Transform target; public Vector3 offset;
        [d]. Update() 함수에서 매 프레임마다 위치를 지정해 준다.
            transform.position = target.position + offset;
        [e]. 씬으로 나가서 속성에 플레이어 위치를 넘겨 주고 거리를 넘겨준다.

    #7. 느낌 살리기
        [a]. 빈 오브젝트를 만든다. WorldSpace
            바닥과 벽 오브젝트 들을 자식으로 둔다.
            y축 45도로 지정한다.
        [b]. Run 애니메이션 속도를 증가시킨다.
*/

/*
2. 3D 쿼터뷰 액션 게임 - 플레이어 점프와 회피

    #1. 코드 정리
        [a]. 입력과 관련된 로직은 GetInput() 함수에 뫃은다.
        [b]. 움직임과 관련된 로직은 Move() 함수에 뫃은다.
        [c]. 회전과 관련된 로직은 Turn() 함수에 뫃은다.
        [d]. Update() 함수에서 호출한다.

    #2. 점프 구현
        [a]. Jump 함수를 만든다.
            점프키가 눌렸는지 확인할 bool 속성을 갖는다. bool jDown;
        [b]. Input 함수에서 점프키를 입력받았을 때 값을 저장한다.
        [c]. Jump 함수에서 jDown이 참일 경우 y축으로 힘을 가한다.
            속성으로 리지드 바디를 받는다.
            AddForce로 힘들 가한다.
                rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impluse);
        [d]. Update() 함수에서 Jump() 함수를 호출한다.
        [e]. 무한 점프를 막기 위해 점프 중인지 체크할 bool 속성을 추가한다. isJump;
        [f]. 점프키가 눌렸음을 확인하면서 동시에 점프 중이 아닌지도 확인하고 y축으로 힘을 가한다.
            점프를 실행할 때 true값을 배정한다.
        [g]. Player가 바닥과 충돌하였을 때 isJump에게 false를 준다.
            충돌 이벤트 함수 OnCollisionEnter 를 만든다.
            충돌체의 tag를 체크 Floor하여 false를 준다.
        [h]. 바닥 오브젝트에 Floor 태크를 만들어서 부착한다.
        [i]. 점프, 착지 애니메이션, 회피를 Player 애니메이터에 등록하고 AnyState로 연결한다.
            Player는 점프를 할 때 점프 애니메이션이 출력되고 바다에 착지되는 순간 착지 애니메이션이 출력된다.
            트리거 파라미터 doJump, doDodge를 추가한다.
            불 파라미터 isJump를 추가한다.
        [j]. Player 스크립트에서 점프를 할 떄 점프 애니메이션을 출력한다.
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            바닥에 다았을 때 착지 파라미터를 전달한다.
            anim.SetBool("isJump", false);
        [k]. ProjectSetting에서 중력을 증가 시킨다.

    #3. 지형 물리 편집
        [a]. WorldSpace와 그 이하 자식들을 static으로 변경한다.
        [b]. 바닥과 벽에 리지드 바디를 부착한다.
            중력 해제, Kinematic, 
        [c]. 물리 Material을 만든다. Wall
            저항력 모두 0으로, 마찰력은 미니멈
            벽의 재질로 지정

    #4. 회피 구현
        [a]. 회피 함수를 만든다. Dodge()
            회피는 점프와 이동 키를 동시에 눌렀을 때 해당 방향으로 빠르게 이동한다.
            현재 회피 중인지 체크할 불 속성을 갖는다. isDodge;
        [b]. 기존 점프 로직에서 제어문 조건을 바꾼다.
            방향 벡터 값이 0일 때를 조건으로 추가 한다.
                moveVec == Vector3.zero
            회피는 반대
        [c]. 코루틴 함수를 만든다.
            점프와 달리 y축으로 뛰지 않고 속도를 2배로 증가 시킨다.
            speed *= 2; true
            애니메이션 파라미터에 트리거를 전달한다.
        [d]. 0.5초 쉬었다가 스피드를 원래대로 바꾸고 false
            speed *= 0.7f;
        [e]. 회피 애니메이션 속도를 더 빠르게 한다.
        [f]. 회피 중에 다른 방향으로 전환할 수 없도록 방향을 고정하기 위한 회피 벡터를 속성으로 만든다.
            Vector3 dodgeVec;
        [g]. 회피를 하는 순간 회피 방향에 이동 방향 값을 대입한다.
        [h]. Move 함수로 가서 현재 회피 중 인지를 확인하고 회피 중 이라면 이동 방향에 회피 방향을 대입한다.
*/