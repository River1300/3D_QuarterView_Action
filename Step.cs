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

/*
3. 3D 쿼터뷰 액션 게임 - 아이템 만들기

    #1. 아이템 준비
        [a]. 프리팹 망치를 하이어라키 창에 드래그드랍
            MeshObject의 y축과 각도를 이쁘게 조정해 준다.

    #2. 라이트 이펙트
        [a]. 망치의 자식으로 빈 오브젝트를 만든다. Light
            컴포넌트 Light를 추가한다.
            y축을 올려서 빛이 잘보이게 한다.
                Type : Point
                Intensity 빛의 세기 조절
                Range 빛의 범위 조절
                Color 를 망치 색과 비슷하게 조절

    #3. 파티클 이펙트
        [a]. 망치의 자식으로 빈 오브젝트를 만든다. Particle
            컴포넌트 Particle System을 추가한다.
        [b]. Renderer에서 Material 을 기본 재질로 지정
        [c]. Emission에서 파티클 출력 갯수를 지정한다.
        [d]. Shape에서 모양을 바꿔준다.
        [e]. Color Over LifeTime에서 색을 지정한다.
        [f]. Size Over LifeTime에서 크기에 커브를 준다.
        [g]. Limit Velocity Over LifeTime에서 저항값을 늘려 준다.
        [h]. Start Life Time, Speed를 지정한다.

    #4. 로직 구현
        [a]. 망치에 리지드바디 컴포넌트와 구체 모양 컴포넌트 두 개를 부착한다.
            구체 1은 플레이어 감지 Trigger
            구제 2는 망치를 바닥에 고정시킬 충돌체
        [b]. Item 스크립트를 만들고 망치에 부착한다.
        [c]. 아이템의 타입을 알기 위해 enum을 활용한다.
            Type { Ammo, Coin, Grenade, Heart, Weapon }
            public Type type;
        [d]. 아이템의 값 public int value;
        [e]. 망치와 마찬가지로 권총, 기관총, 총알, 돈1,2,3, 수류탄, 하트를 만든다.
        [f]. 스크립트로 가서 Update 함수에서 오브젝트에게 회전을 준다.
            transform.Ratate(Vector3.up * 25 * Time.deltaTime);

    #5. 프리팹 저장
        [a]. 태그를 추가한다.
            Item, Weapon
        [b]. 아이템에 태그를 부착한다.
        [c]. 프리팹 폴더를 만들어서 아이템을 에셋화 한다.
        [d]. 에셋들의 좌표를 초기화
*/

/*
4. 3D 쿼터뷰 액션 게임 - 드랍 무기 입수와 교체

    #1. 오브젝트 감지
        [a]. 플레이어가 아이템에 접근 하였는제 체크하는 함수를 만든다.
            OnTriggerStay OnTriggerExit
        [b]. 플레이어와 가까이에 있는 오브젝트를 활용하기 위해 속성을 만든다.
            GameObeject nearObject;
        [c]. Stay 함수에서 가까이에 있는 오브젝트의 tag가 Weapon일 경우 nearObject에 배정한다.
        [d]. Exit 함수에서 가까이에 있는 오브젝트의 tag가 Weapon일 경우 nearObject에 null을 배정한다.

    #2. 무기 입수
        [a]. 상호작용 키를 Input Manager에 e키로 등록한다. Interaction
        [b]. 속성으로 해당 키가 눌렸는지 체크할 불 변수, 플레이어의 무기 관련 배열 변수와 무기 보유 여부를 체크할 불 배열을 만든다.
            bool iDown; public GameObject[] weapons; public bool[] hasWeapons;
        [c]. 입력 받는 함수에서 상호작용 버튼의 입력을 저장한다.
        [d]. 상호작용 함수를 만든다. Interaction
            상호작용 버튼이 눌린 상태이면서 동시에 nearObject가 null이 아닐 경우 점프, 회피중이 아닌 경우
                nearObject의 태그가 Weapon일 경우 해당 오브젝트로 부터 Item 스크립트를 받아 온다.
                Weapon마다 value 속성에 각기 다른 값을 넣었다. 이 값을 지역 변수로 저장한다.
                    int weaponIndex = item.value;
                    hasWeapons[weaponIndex] = true;
                    Destroy(nearObject);
        [e]. 씬으로 나가서 HasWeapons에 무기 갯수를 지정한다.

    #3. 무기 장착
        [a]. Player객체의 자식 RightHand에 3D 오브젝트 실린더를 만든다. Weapon Point
            손 안으로 위치를 대략적으로 맞추어 준다.
            크기를 4로 맞춘다.
            콜라이더 컴포넌트는 제거하고 MeshRenderer 컴포넌트는 비활성화 한다.
            3가지 무기를 실린더의 자식으로 등록한다.
                무기를 보며 WeaponPoint의 위치를 수정한다.
            무기들을 모두 비활성화 한다.
        [g]. 플레이어 스크립트의 속성 Weapons에 비활성화 한 무기 3가지를 넣어 준다.

    #4. 무기 교체
        [a]. 1,2,3번 키에 따라 보유하고 있는 무기를 교체할 예정이다.
            버튼 입력 불 변수를 만든다. sDown1,2,3;
        [b]. 입력 함수에서 Swap1,2,3 입력을 받아 저장한다.
        [c]. Input Manager에 3개의 키 입력을 추가한다.
        [d]. 무기 교체 함수를 만든다. Swap
            먼저 배열의 인덱스를 지역 변수로 만들어 준다. weaponIndex = -1;
            눌린 버튼에 따라서 인덱스 값이 지정된다.
            sDown1 || sDown2 || sDown3 이 눌리고 점프 회피 중이 아닐 때
                무기 배열의 인덱스 값을 활성화 한다.
                    weapons[weaponIndex].SetActive(true);
        [e]. 현재 손에 쥐고 있는 오브젝트를 알기위해 게임 오브젝트 속성을 갖는다.
            GameObject equipWeapon;
        [f]. 무기를 활성화 할 때 해당 무기를 속성에 저장하고 활성화 한다.
            그리고 그 이전에 끼고 있던 무기는 비활성화 한다.
                equipWeapon = weapons[weaponIndex];
            단 빈손일 경우에는 비활성화 로직은 실행하지 않는다.
                if(equipWeapon != null)
        [g]. 무기 교체 애니메이션 클립을 애니메이터에 등록한다.
            AnyState에 연결한다.
            doSwap 트리거 파라미터를 추가한다.
        [h]. 무기를 활성화 할 때 파라미터 전달
        [i]. 무기를 교체하는 동안에 움직임의 제약을 걸기 위해 교체 중임을 체크할 불 속성을 만든다.
            bool isSwap;
        [j]. 코루틴 함수를 만들어서 무기를 교체하고 0.4초 뒤에 false 이전에는 true
        [k]. 현재 사용중이 무기의 인덱스를 속성으로 갖는다.
            int equipWeaponIndex = 1;
        [l]. 없는 무기는 교체해서는 안되고 동일한 무기는 교체할 필요가 없다.
            if(sDown1 && (!hasWeapons[0] || equipWeaponIndex== 0)) return;
            나머지 무기도 마찬가지로
        [m]. 인덱스는 무기를 교체할 때 배정한다.
            equipWeaponIndex = weaponIndex;
*/

/*
5. 3D 쿼터뷰 액션 게임 - 아이템 먹기 & 공전물체 만들기

    #1. 변수 생성
        [a]. 플레이어 스크립트로 가서 탄약 동전 체력 수류탄 변수를 속성으로 갖는다.
            public int ammo; public int coin; public int health; public int hasGrenade;
        [b]. 각 수치의 최대값도 속성으로 만든다. max...
        [c]. 씬으로 나가서 최대값을 지정한다.

    #2. 아이템 입수
        [a]. 아이템과 접촉하였을데 획득하도록 트리거 함수를 만든다.
            OnTriggerEnter
        [b]. 만약 Item 태그와 접촉했다면 switch문을 통해 아이템 타입 별로 로직을 작성한다.
        [c]. Item.Type.Ammo
            ammo += item.value;
            단 max값을 넘을 경우 max값을 배정
        [d]. Item.Type.Coin, Heart, Grenade도 마찬가지로 작성
        [e]. switch문을 빠져 나온뒤 충돌한 오브젝트 삭제

    #3. 공전물체 만들기
        [a]. 플레이어가 획득한 수류탄이 플레이어를 공전하도록 먼저 속성으로 공전할 오브젝트 배열을 만든다.
            public GameObject[] grenades;
        [b]. 빈 오브젝트를 만든다. Grenade Group
            자식으로 빈 오브젝트를 만들어서 동서남북으로 위치를 지정해 준다.
        [c]. 수류탄 프리팹을 각 빈 오브젝트의 자식으로 한 개씩 넣어 준다.
            수류탄의 각도를 z 30
        [d]. Material을 교체한다.
        [e]. 수류탄의 Mesh Object에게 파티클 컴포넌트를 부착한다.
            Emission Distance 10으로 지정하여 움직임에 따라 발생하도록 한다.
            Simulation Space를 World로 교체하여 입자가 수류탄을 따라가지 않도록 한다.
            그 외의 옵션을 조정한다.

    #4. 공전 구현
        [a]. Orbit 스크립트 작성하고 동서남북 빈 오브젝트에 각각 부착
        [b]. 속성으로 플레이어 위치와 공전 속도, 플레이어와 수류탄 간의 거리를 갖는다.
            public Transform target; public float orbitSpace; Vector3 offSet;
        [c]. Update함수에서 플레이어를 기준으로 주위를 돈다.
            transform.RotateAround(target.position, Vector3.up, orbitSpeed * Time.deltaTime);
        [d]. 이때 RotateAround함수가 수류탄의 위치를 지정하여 플레이어 움직임을 똑바로 따라가지 못하고 있다.
        [e]. Start함수에서 offSet 속성에 거리 값을 저장한다.
            offSet = transform.position - target.position;
        [f]. Update함수에서 플레이어와 수류탄의 거리 값을 이용하여 수류탄의 위치를 지정해 준다.
            transform.position = target.position + offSet;
        [g]. 회전 로직 이후에 거리 값을 다시 한번 갱신하여 목표와의 거리를 지속적으로 유지해 준다.
            offSet = transform.position - target.position;
        [h]. 자식으로 등록되어 있는 수류탄 오브젝트를 Player의 속성 grenades에 넣어 준다.
            그리고 오브젝트들은 비활성화 시킨다.
        [i]. Player 스크립트에서 수류탄을 획득하는 로직에서 수류탄을 획득할 때 오브젝트를 활성화 시킨다.
            grenade[hasGrenades].SetActive(true);
*/