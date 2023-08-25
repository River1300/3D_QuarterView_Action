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
            speed *= 0.5f;
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
            구체 2는 망치를 바닥에 고정시킬 충돌체
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
        [k]. 현재 사용중인 무기의 인덱스를 속성으로 갖는다.
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
            public Transform target; public float orbitSpeed; Vector3 offSet;
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

/*
6. 3D 쿼터뷰 액션 게임 - 코루틴으로 근접 공격 구현하기

    #1. 변수 생성
        [a]. 무기 정보를 갖는 스크립트를 만든다. Weapon
            플레이어 자식으로 두었던 무기들에게 부착한다.
        [b]. 무기의 타입을 열거형으로 갖는다. Type { Melee, Range }
            public Type type;
            무기의 공격력과, 공격 속도, 공격 범위, 공격 효과를 속성으로 갖는다.
            public int damage; public float rate; public BoxCollider meleeArea; public TrailRenderer trailEffect;

    #2. 근접 공격 범위
        [a]. 씬으로 나가서 속성을 채운다.
            망치 오브젝트에게 박스 콜라이더를 부착한다. 콜라이더 위치를 타격 위치로 조정한다.
            태그 Melee를 추가하고 망치에 적용한다.
            Trigger 체크

    #3. 근접 공격 효과
        [a]. 망치의 자식으로 빈 오브젝트를 만든다. Effect
            Trail Renderer 컴포넌트를 부착한다.
                움직이는 물체의 꼬리처럼 이펙트가 생성된다.
                Material 지정
                그래프에 Add Key로 커브를 주어 꼬리 모양처럼 만든다.
                Time을 줄여 준다.
                Min Vertax Distance를 높여서 조금 더 각진 모양으로 만들어 준다.
                색을 바꾼다.
                이펙트 위치를 조절한다.
        [b]. Weapon 속성에 이펙트를 넣어 준다.
        [c]. 이펙트의 Trail Renderer 비활성화
        [d]. 망치의 박스 콜라이더 비활성화

    #4. 공격 로직( 코루틴 )
        [a]. Weapon 스크립트로 가서 무기 사용 함수를 만든다.
            public void Use()
        [b]. 스크립트 부착된 무기의 타입이 Melee일 경우 휘두루는 코루틴 함수는 호출한다.
        [c]. IEnumerator Swing()
            한 프레임 쉬고 콜라이더와 이펙트를 활성화 한다.
            meleeArea.enabled = true;
            trailEffect.enabled = true;
            무기 공속에 맞추어 잠시 쉬어준뒤 콜라이더 비활성화 하고 잠시 쉬었다가 이펙트도 비활성화

    #5. 공격 실행
        [a]. Player 스크립트에 공격 키 입력 불, 공격 딜레이, 공격 준비 속성을 만든다.
            bool fDown; float fireDelay; bool isFireReady;
        [b]. 기존에 지정해 둔 현재 장비 속성을 GameObject에서 Weapon올 바꿔준다.
            Weapon equipWeapon;
        [c]. 입력 함수에서 무기 공격 키를 입력 받는다.
        [d]. Attack() 함수를 만든다.
            손에 무기가 있는지 먼저 확인한다.
                if(equipWeapon == null) return;
            무기 딜레이에 시간을 더해준다.
                fireDelay += Time.deltaTime;
            공격 준비 속성에 딜레이 시간을 체크한다.
                isFireReady = equipWeapon.rate < fireDelay;
            공격 버튼을 눌렀고 공격 준비가 되었고 회피중이 아니고, 교체 중이 아닐 때
                Weapon스크립트의 Use() 함수를 호출한다.
                    equipWeapon.Use();
                애니메이션 트리거를 전달한다.
                공격을 하였으니 fireDelay는 초기화
        [e]. 애니메이션 클립을 애니메이터에 추가한다.
            doSwing 트리거를 추가한다.
*/

/*
7. 3D 쿼터뷰 액션 게임 - 원거리 공격 구현

    #1. 총알, 탄피 만들기
        [a]. 빈 오브젝트를 만든다. Bullet HandGun
            트레일 렌더러 컴포넌트 부착
                속성을 날아가는 총알처럼 조정해 준다.
        [b]. 리지드 바디와 스피어 콜라이더 부착
            콜라이더의 크기를 대략 총알 크기 정도록 조정해 준다.
        [c]. 핸드건 총알을 복사한다. Bullet SubMachineGun
            약간의 변화를 준다.
        [d]. 탄피 프리팹을 하이어라키 창에 등록
            MeshObject의 크기를 0.5
        [e]. 탄피에게 리지드바디와 박스콜라이더를 부착한다.
            콜라이더 크기 조정
        [f]. 총알 스크립트를 만든다. Bullet
        [g]. 속성으로 데미지를 갖는다. public int damage;
        [h]. 충돌 이벤트 함수를 만든다. OnCollisionEnter
            바닥 태그와 충돌하였을 경우 3초 뒤에 사라진다.
            그게 아니라 벽에 충돌하였을 경우 바로 제거된다.
        [i]. Bullet 스크립트를 3개의 총알, 탄피에 부착한다.
        [j]. 프리팹으로 저장한다.

    #2. 발사 구현
        [a]. 발사 애니메이션 클립을 애니메이터에 등록한다.
            doShot 트리거 추가
        [b]. Player 스크립트 Attack() 함수에 무기의 타입에 따른 각기 다른 로직을 만든다.
            삼항 연산자로 각기 다른 트리거를 애니메이터에 전달한다.
                anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
        [c]. 씬으로 나가 플레이어 자식으로 두었던 총 오브젝트들의 속성을 채워준다.
            총알이 발사되는 위치와 탄피가 나오는 위치를 지정해 준다.
                Weapon 스크립트에 속성으로 준다.
                    public Transform bulletPos; public GameObject bullet; public Transform bulletCasePos; public GameObejct bulletCase;
        [d]. Player의 바로 밑 자식으로 빈 오브젝트를 만든다. Bullet Pos
            총알이 발사될 위치를 지정해 준다.
            총의 자식으로 빈 오브젝트를 만든다. Case Pos
                게임 씬의 툴 좌표 기준을 Global에서 Local로 바꿔준다.
                탄피가 나오는 위치를 지정해 준다.
        [e]. Weapon 속성에 위치들과 프리팹들을 넣어 준다.
        [f]. Weapon 스크립트 Use 함수에서 플레이어의 공격키에 맞추어 출력되도록 되었다.
            Type.Range일 경우를 추가한다.
        [g]. Shot 코루틴 함수를 만든다.
            총알을 발사하고 일정 시간 쉬었다가 탄피 배출
                GameObject instanceBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotetion);
                발사 이므로 힘을 가해야 한다. 리지드바디 컴포넌트를 받아와서 속도를 지정해 준다.
                    Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
                    bulletRigid.velocity = bulletPos.forward * 50;
            탄피 배출도 총알과 마찬가지로 인스턴스화 하고 리지드바디를 받아온다.
            그 이후 탄피가 배출될 방향을 지정해 준다.
                Vector3 caseVec = bulletCasePos.forward * 랜덤값 + Vector3.up * 랜덤값
            그리고 일시적인 힘을 가해 탄피를 배출한다.
            탄피에 약간의 회전 값을 추가로 준다.
                rigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
        [h]. Wall 태그를 만들고 벽에 등록한다.

    #3. 재장전 구현
        [a]. Weapon 스크립트에서 전체 탄약과 현재 탕약을 속성으로 갖는다.
            public int maxAmmo; public int curAmmo;
        [b]. Use함수에서 Type.Range이면서 동시에 총알이 남아있을 때 코루틴을 호출하도록 한다.
            코루틴을 호출하기 전에 총알을 --;
        [c]. Player스크립트에 재장전 키와 재장전 중 속성으로 만든다. bool rDown; bool isReload;
        [d]. Reload() 함수를 만든다.
            현재 손에 든 무기가 없다면 반환 equipWeapon == null
            현재 손에 든 무기의 타입이 근접이여도 반환 type == Weapon.Type.Melee
            플레이어가 보유한 총알이 없다면 반환
            rDown 이 true이고 점프나 회피, 재장전 중이 아니고, 공격 준비 상태일 때 파라미터 전달
            isReload = true; 
            장전 완료 함수를 만들어서 인보크로 호출한다.
        [e]. InputManager에서 Reload 로 r 키를 만든다.
        [f]. 재장전 애니메이션 클립을 애니메이터에 등록
            doReload 트리거 생성
        [g]. ReloadOut
            플레이어가 보유한 총알이 해당 무기의 재장전 총알 갯수와 비교하여 재장전을 실행한다.
                int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
            equipWeapon.curAmmo = reAmmo;
            ammo -= reAmmo;
            isReload = false;
        [h]. 플레이어 자식인 총들의 장전 Ammo값을 지정해 준다.

    #4. 마우스 회전
        [a]. Player 스크립트로 가서 메인 카메라 속성을 만든다.
            public Camera followCamera;
            메인 카메라를 넣은다.
        [b]. Turn() 함수로 가서 마우스에 의한 회전 로직을 작성한다.
            Ray를 활용하여 스크린에 찍인 좌표로 회전하도록 한다.
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if(Physics.Raycast(ray, out rayHit, 100))
                레이가 다은 지점과 플레이어의 위치를 빼기 연산하여 방향을 구한다.
                    Vector3 nextVec = rayHit.point - transform.position;
                transform.LookAt(transform.position + nextVec);
            단, 마우스 클릭 fDown이 있을 때만 마우스에 의한 회전이 실행되도록 제어문을 만든다.
        [c]. 플레이어가 높이가 있는 오브젝트르 바라볼 때 하늘을 보지 않도록 nextVec.y를 0으로 고정한다.
*/

/*
8. 3D 쿼터뷰 액션 게임 - 플레이어 물리문제 고치기

    #1. 자동 회전 방지
        [a]. Transform의 위치를 매 프레임마다 지정하여 움직이는 플레이어가 다른 물체와 충돌하였을 때
            플레이어의 리지드바디에 힘이 가해저 비정상적인 움직임이 발생한다.
        [b]. FixedUpdate 함수를 만든다.
            회전 방지 함수를 만든다. FreezeRotation
        [c]. 리지드바디의 회전 속도를 0으로 고정 시킨다.
            rigid.angularVeclocity = Vector3.zero;

    #2. 충돌 레이어 설정
        [a]. 오브젝트들 간에 불필요한 충돌을 무시하도록 한다.
        [b]. Floor, Player, PlayerBullet, BulletCase 레이어를 추가하여 오브젝트에 등록해 준다.
        [c]. Physics 세팅에서 플레이어와 플레이어 총알, 탄피의 충돌을 무시한다.

    #3. 벽 관통 방지
        [a]. 벽앞에 멈추는 기능을 할 함수를 만든다.
            StopToWall
        [b]. 플레이어의 앞으로 짧은 길이의 레이를 쏜다.
            Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        [c]. 벽 충돌을 감지할 불 속성을 만든다. bool isBoarder;
        [d]. 속성에 벽과의 충돌을 체크하도록 한다.
            isBoarder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
        [e]. Move함수에서 이동을 하기 위해 현재 위치에 미래의 위치를 더하는 작업을 맊는다.
        [f]. 벽에 Wall 레이어를 추가해 준다.

    #4. 아이템 물리 충돌 제거
        [a]. 플레이어가 아이템의 지면 고정 콜라이더와 충돌하면 아이템에 예측하기 어려운 힘이 가해진다.
        [b]. Item 스크립트에서 리지드바디와 스피어콜라이더를 속성으로 받는다.
        [c]. OnCollisionEnter 함수를 만든다.
            바닥에 다았을 때 리지드바디의 isKinematice을 활성화 시키고 콜라이더를 비활성화 한다.
*/

/*
9. 3D 쿼터뷰 액션 게임 - 피격 테스터 만들기

    #1. 오브젝트 생성
        [a]. 3D 큐브를 만든다.
            크기를 알맞게 지정한다.
        [b]. 리지드바디
            프리즈 로테이션 x,z
        [c]. Enemy 스크립트를 만들고 큐브에 부착한다.

    #2. 충돌 이벤트
        [a]. 속성으로 체력과 리지드바디, 박스 콜라이더를 갖는다.
            public int maxHealth; public int curHealth; Rigidbody rigid; BoxCollider boxCollider;
        [b]. 충돌 이벤트 함수를 만든다.
            OnTriggerEnter
            충돌한 태그가 Melee 일때
            충돌한 태그가 Bullet 일때
        [c]. 충돌한 오브젝트로 부터 Weapon 스크립트를 받아온다.
            Weapon weapon = other.GetComponent<Weapon>();
            체력을 줄인다.
                curHealth -= weapon.damage;
            총알은 총알 스크립트로
        [d]. 총알 프리팹에 Bullet 태그와 트리거 체크를 해준다.
        [e]. Bullet 스크립트에서 OnTriggerEnter로 벽 태그를 지정한다.

    #3. 피격 로직
        [a]. Enemy의 피격 함수를 코루틴으로 만든다. OnDamage
        [b]. 먼저 색을 바꾼다.
            Material mat 속성을 갖는다.
                mat = GetComponent<MeshRenderer>().material;
            mat.color = Color.red;
        [c]. 0.1초 쉬었다가, 체력이 다 달았는제 제어문으로 확인한다.
        [d]. 만약 체력이 남아 있다면 색을 다시 원상 복구 Color.white;
        [e]. 만약 죽었다면 Color.gray;
            그리고 4초 뒤에 죽인다.
                Destroy(gameObject, 4);
        [f]. 죽은 상태에서는 추가 피격을 받지 않도록 물리 레이어를 추가한다.
            EnemyDead 레이어를 추가하고 Physics 세팅에서 Floor, Wall을 제외한 모든 것과 충돌을 무시한다.
        [g]. 몬스터가 제거되기 전에 레이어를 바꿔준다.
            gameObject.layer = 번호;

    #4. 넉백 추가
        [a]. 죽을 때 넉백을 주기 위해 Melee와 Range에서 피격 방향을 저장한다.
            Vector3 reactVec = transform.position - other.transform.position;
        [b]. OnDamage에서 매개변수로 벡터를 받는다.
        [c]. 리지드바디로 힘을 가해서 넉백을 준다.
            reactVec = reactVec.normalized;
            reactVec += Vector3.up;
            rigid.AddForce()
        [d]. 총알의 경우 Enemy와 충돌하여 방향을 구한 뒤에 사라진다.
*/

/*
10. 3D 쿼터뷰 액션 게임 - 수류탄 구현하기

    #1. 오브젝트 생성
        [a]. 수류탄 프리팹을 하이어라키창에 등록한다.
        [b]. GrenadeEffect 파티클을 수류탄 자식으로 등록한다.
        [c]. 리지드바디와 스피어 콜라이더를 부착한다.
        [d]. 피직스 매터리얼을 추가하여 모든 저항 값을 1로 지정하여 콜라이더에 부착한다.
        [e]. MeshObject에 트레일 렌더러를 추가한다.
            마테리얼, 커브, 색, 시간을 지정한다.
        [f]. 수류탄의 레이어를 PlayerBullet으로 지정한다.
        [g]. 프리팹화 한다.

    #2. 수류탄 투척
        [a]. 플레이어 스크립트에서 수류탄 프리팹 속성과 g키 불 속성을 갖는다.
            public GameObject grenadeObj; bool gDown;
        [b]. 입력 키를 받는다.
        [c]. Grenade() 함수를 만든다.
            수류탄이 없을 때 반환
            g버튼을 눌렀고 재장전이나 무기 교체를 하지 않을 때 수류탄을 던진다.
        [d]. 마우스를 클릭한 자리에 수류탄을 던진다.
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if(Physics.Raycast(ray, out rayHit, 100))
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 15;
                수류탄 인스턴스화
                    GameObject instantGrenade = Instantiate(grenadeObj, transform.position, ...);
                리지드 바디를 받아오고 힘을 주어 던진다.
                회전도 
                    rigid.Grenade.AddTorque(Vector3.back * 10, 임펄스);
                그 이후에는 수류탄 개수 차감, 공전하는 수류탄 하나 제거
                    grenades[hasGrenades].비활성화
        [e]. 프리팹의 수류탄 이펙트를 비활성화 시켜 놓는다.

    #3. 수류탄 폭발
        [a]. Grenade 스크립트 생성
        [b]. 수류탄의 자식 MeshObject와 Effect를 활성화 비활성화 하기 위해 속성으로 갖는다.
            public GameObject meshObj; public GameObject effectObj; public Rigidbody rigid;
        [c]. 코루틴 함수를 만들어서 3초 뒤에 폭발하도록 한다.
            rigid.velocity 속도 0, angularVelocity 0
            meshObje 비활성화 effectObje 활성화
        [d]. Start 함수에서 코루틴 함수 호출

    #4. 수류탄 피격
        [a]. 다시 수튜탄 스크립트의 코루틴 함수로 가서 동그란 레이를 발사하도록 한다.
        [b]. 배열로 충돌한 모든 적을 받아온다.
            RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, 15, Vector3.up, 0, LayerMast.GetMask("Enemy));
        [c]. 반복문으로 몬스터 스크립트를 받아, 수류탄 피격 함수를 호출한다.
            foreach(RaycastHit hitObj in rayHits)
        [d]. Enemy스크립트에서 수류탄 피격 함수를 만든다.
            public void HitByGrenade(Vector3 explosionPos)
            체력을 100 차감하고 피격 위치 벡터를 구하고, 피격 코루틴을 호출한다.
        [e]. 수류탄에 의해 사망할 경우 더 격렬하게 사망하도록 한다.
            OnDamage함수의 매개 변수로 수류탄인지를 받는다. bool isGrenade
            죽는 로직에서 수류탄인지 제어문을 만든다.
                수류탄일 경우 Vector3.up을 더 증가 시킨다.
                회전을 주기 위해 몬스터의 freezeRotation = false로 바꿔준다.
                rigid.AddTorque(reactVec * 15, 임펄스);
        [f]. 다시 수류탄 스크립트로 가서 반복문을 탈출 한 뒤 5초뒤 수류탄을 제거한다.
*/

/*
11. 3D 쿼터뷰 액션 게임 - 목표물을 추적하는 AI 만들기

    #1. 오브젝트 생성
        [a]. 몬스터A 프리팹을 하이어라키창에 등록
        [b]. 리지드바디, 박스 콜라이더 부착, Enemy 스크립트 부착
            FreezeRotetion x, z
            박스 콜라이더 위치와 크기 조정, 체력 지정
        [c]. 기존 Enemy 스크립트의 마테리얼 초기화에서 GetComponentInChildren로 수정한다.
        [d]. Enemy 객체에 Enemy 태그와 레이어를 등록해 준다.

    #2. 네비게이션
        [a]. EnemyA에 Nav Mesh Agent 컴포넌트를 추가한다.
        [b]. 스크립트로 가서 Player 목표물과 네비게이션을 속성으로 갖는다.
            using UnityEngine.AI; public Transform Target; NavMeshAgent nav;
            네비게이션 초기화
        [c]. Update 함수에서 목표를 추적한다.
            nav.SetDestination(target.position);
        [d]. Window -> AI -> Navigation
            Bake -> Bake
        [e]. 몬스터가 물리적인 충돌이 발생할 경우 리지드바디에 의해 움직이게 되는데 이것을 막는다.
        [f]. FixedUpdate에서 속력 값을 0으로 고정해 준다.
            FreezeVelocity()
                rigid.velocity, angularVelocity 모두 Vector3.zero;

    #3. 애니메이션
        [a]. 애니메이터 컨트롤러를 만든다. Enemy A
        [b]. Enemy A의 애니메이션을 모두 컨트롤러에 등록한다.
        [c]. 트랜지션을 연결한다.
            isWalk, isAttack, doDie 파라미터 추가
        [d]. Enemy 스크립트에 애니메이터, 추격 중 불 속성을 추가한다. isChase;
        [e]. 추격 함수를 추가한다.
            ChaseStart()
                추격 중일 때는 불 값으로 true, 파라미터 전달
        [f]. Update 함수에서 플레이어를 추격하는 것은 isChase가 true일 때만 하도록 제어문을 만든다.
        [g]. 인보크로 Awake함수에서 ChaseStart를 호출한다.
        [h]. 몬스터가 죽는 로직에서 파라미터 전달, isChase false, 네비게이션 비활성화
            nav.enabled = false;
        [i]. 리지드바디의 속도를 고정하는 로직에서도 추적 중일 때만 고정하도록 제어문을 만든다.
*/

/*
12. 3D 쿼터뷰 액션 게임 - 다양한 몬스터 만들기

    #1. 플레이어 피격
        [a]. Player 스크립트에서 Item과 충돌하는 것 이외에 EnemyBullet과 충돌하는 경우를 추가한다.
        [b]. 충돌한 적 총알로 부터 총알 스크립트를 받아와서 health를 차감한다.
        [c]. 피격 코루틴 함수를 만든다. OnDamage()
            피격 직후 일정 시간의 무적 타임이 필요하므로 무적타임 bool 속성을 갖는다. bool isDamage;
            처음 피격을 당하면 true;
            1초간 무적시간으로 지정하고 false;
        [d]. 적 총알과의 충돌 로직은 isDamage가 false일 때만 실행하도록 제어문을 만든다.
        [e]. 피격시 플레이어의 색을 바꾸기 위해 마테리얼 속성을 갖는다.
            MeshRenderer[] meshs;
            meshs = GetComponentsInChildren<...>();
        [f]. isDamage 가 true일 때 반복문으로 Player의 파츠를 순회하며 색을 바꿔준다.
            foreach(MeshRenderer mesh in meshs)
                mesh.material.color = Color.yellow;
            일정 시간이 지나고 원상 복구 한다.
        [g].빈 오브젝트를 만든고 박스 콜라이더를 부착, Bullet 스크립트를 부착한다. EnemyBullet

    #2. 몬스터 움직임 보완
        [a]. Enemy 스크립트에서 Update 함수로 플레이어를 추적하였는데, 이때 제어문을 수정한다.
            nav.enabled : 네비게이션이 활성화 되어 있을 때만 추적한다.
            그리고 네비게이션이 멈추는 조건으로 isChase가 false인 경우를 추가한다.
            nav.isStopped = !isChase;

    #3. 일반형 몬스터
        [a]. EnemyBullet 객체를 Enemy A의 자식으로 등록한다.
            위치를 몬스터보다 조금더 앞쪽으로 지정해 준다.
        [b]. Enemy 스크립트에 박스 콜라이더, 공격중 속성으로 만든다.
            public BoxCollider meleeArea; public bool isAttack;
        [c]. 플레이어를 공격할 수 있는 공격 범위에 들어왔는지 체크하는 함수를 만든다.
            Targeting(), FixedUpdate() 함수에서 호출한다.
        [d]. 스피어 캐스트로 플레이어를 감지한다.
            먼저 감지 범위를 변수로 만든다.
                float targetRadius = 1.5f; float targetRange = 3f;
        [e]. Ray를 발사하고 플레이어를 감지한다.
            RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));
            플레이어가 감지되었다면 구조체에 길이가 들어올 것이고 몬스터가 공격 중이 아니라면 공격을 하면된다.
            if(rayHits.Length > 0 && !isAttack)
        [f]. 공격 코루틴 함수를 만든다. Attack()
            먼저 정지를 한 다음, 애니메이션과 함께 공격 범위를 활성화하여 공격한다.
                isChase = false; isAttack = true; anim.SetBool(...); meleeArea.enabled = true;
            공격 애니메이션 출력 중 때리는 타이밍에 공격 범위를 활성화 하고
            잠시 쉬었다가 활성화 하였던 것을 비활성화 하고 다시 추적한다.
        [g]. 씬으로 나가서 EnemyBullet의 박스 콜라이더를 비활성해 해 놓는다.

    #4. 돌격형 몬스터
        [a]. Enemy B 프리팹을 하이어라키 창에 등록
            리지드바디, 박스콜라이더, Enemy 스크립트, Nav Mesh Agent 컴포넌트 부착
            속성 값을 채워 준다.
            Enemy A의 EnemyBullet을 Enemy B에도 적용 시킨다.
        [b]. 애니메이터 컨트롤러 Enemy A를 복사하여 B를 MeshObject에 부착한다.
        [c]. Nav Mesh Agent의 속성 Angular Speed A,B 360, Acceleration A는 20, B는 50
        [d]. enum으로 몬스터의 타입을 속성으로 갖는다.
            public enum Type { A, B, C }; public Type enemyType;
        [e]. Targeting 함수에서 플레이어를 감지하는 로직에 Type 별로 다르게 작성한다.
            switch 문을 활용하여  타입별로 각기 다른 반지름과 공격 범위를 지정해 준다.
                Radius = 1f; Range 12f;
        [f]. Attack 코루틴에서도 switch문으로 타입별 다른 공격을 실행한다.
            B타입은 0.1초 잠깐 멈추었다가 AddForce로 transform.forward로 플레이어에게 발사된다.
            이때 콜라이더를 활성화 한다.
            0.5초 뒤에 velocity를 0으로 하여 멈춘다. 콜라이더 비활성화
            그리고 2초간 대기

    #5. 원거리형 몬스터
        [a]. Enemy C 프리팹을 하이어라키 창에 등록
            컴포넌트들을 부착
        [b]. Missile 프리팹을 하이어라키 창에 등록
            MeshObject의 y축 값 3
                몬스터 방향과 동일하게 y 90
        [c]. Missile 스크립트를 만들고 MeshObject에 부착
            Update 함수에서 미사일을 회전 시킨다.
                transform.Rotate(Vector3.right * 30 * Time.de...);
        [d]. Missile의 자식으로 빈 오브젝트를 만든다. Effect
            위치를 미사일 뒤로 지정해 주고 파티클 시스템 컴포넌트를 부착한다.
                마테리얼, 이미션, 색, 크기, 모양, 시간 등을 조정해 준다.
        [e]. Missile에 리지드바디, 박스 콜라이더, Bullet 스크립트를 부착해 준다. 트리거
            Tag와 Layer도 지정해 준다.
        [f]. Missile을 프리팹화 한다.
        [g]. Enemy 애니메이터 컨트롤러를 복사하여 C로 지정한다.
            애니메이션 스테이트에 클립을 교체해 준다.
        [h]. 스크립트의 속성을 채워 준다.
        [i]. 미사일 프리팹을 담아둘 속성을 갖는다.
            public GameObject bullet;
        [j]. Targeting
            Radius = 0.5f; Range = 25f;
        [k]. Attack
            0.5초 쉬었다가 미사일을 인스턴트화 한다.
                GameObject instantBullet = ...
            리지드바디를 받아와서 transform.forward로 velocity를 지정해 준다.
            그리고 2초간 대기
        [l]. Enemy와 EnemyBullet은 서로 충돌하지 않도록 Physics에서 세팅한다.
        [m]. Missle의 리지드바디의 UseGravity를 해제
        [n]. EnemyBullet 태그를 부착한 오브젝트들 중 Missile 만이 유일하게 리지드바디를 가지고 있다.
            플레이어 스크립트로 가서 EnemyBullet과 충돌 하였을 때 리지드바디가 있는지 체크하여 있다면 제거한다.
                if(other.GetComponent<Rigidbody>() != null) Destroy(other.gameObject);
        [o]. 플레이어가 벽에 붙어있을 때 근접 공격 몬스터가 잘못 공격해서 벽에 닿으면 콜라이더가 제거될 수 있다.
            Bullet 스크립트에서 근접 공격 범위가 제거되지 않도록 플래그를 만든다.
                public bool isMelee;
            if(!isMelee && other.gameObject.tag ...)
        [p]. 근접 공격 오브젝트에 속성을 체크해 준다.
*/

/*
13. 3D 쿼터뷰 액션 게임 - 다양한 패턴을 구사하는 보스 만들기

    #1. 보스 기본 세팅
        [a]. Boss 프리팹을 하이어라키 창에 등록
        [b]. MeshObject에 애니메이터 컨트롤러를 복사하여 부착한다.
            Walk, Attack 스테이트, 파라미터 제거
            Shot, BigShot, Taunt 클립을 등록하고 Any State로 연결
            각각의 트리거 생성
        [c]. Enemy D에 리지드바디와 박스 콜라이더, 네비게이션 부착
            Nav Speed 40, Angular Speed 0, Acceleration 60
            보스몹 크기가 줄어들때 해결법
            원인 : Animation에 scale이 1,1,1로 설정 되있는 경우에 그럼
            해결법 : 제일 간단한 해결법중 하나로 보스몹 오브젝트 안에 empty object 하나 만든 다음
            object scale을 3으로 변경해주고 자식으로 Mesh Object를  두면 다시 커짐.
        [d]. 보스의 귀에서 미사일을 발사하기 위해 위치를 잡아 준다.
            Enemy D의 자식으로 빈 오브젝트를 만들고 미사일이 발사될 위치로 이동 시킨다.
                Missile Port A, B
        [e]. 보스의 공격 패턴 중 높이 점프하였다가 뭉게버리는 패턴이 있다.
            이를 위해 보스의 발에 타격 범위를 지정해 주자.
                Enemy D의 자식으로 빈 오브젝트를 만들고 박스 콜라이더 부착 Melee Area
                    트리거
                박스 콜라이더는 비활성화 시켜 놓는다.
        [f]. Enemy D는 Enemy 태그와 레이어, Melee Area는 EnemyBullet

    #2. 투사체(미사일) 만들기
        [a]. Missile Boss 프리팹, Boss Rock 프리팹을 하이어 라키 창에 등록
        [b]. Missile Boss의 MeshObject y 축 위치 조정
            z축 방향에 맞추어 미사일 y축 값을 회전
            Missile 스크립트 부착
        [c]. Missile Boss의 자식으로 빈 오브젝트를 만든다. Effect
            파티클 시스템 부착
            위치, 방향, 랜더러, 쉐이프, 색, 사이즈, 생명, 속도 지정
            Simulation Space를 World로 지정
        [d]. Missile Boss에 리지드바디, 박스 콜라이더, 네비게이션 부착
            Use Gravity 해제, 트리거
            EnemyBullet 태그와 레이어
        [e]. 스크립트 생성 BossMissile
            MonoBehaviour을 대신해서 Bullet 클래스를 상속 받는다.
            네비게이션을 사용하기 때문에 AI헤더를 추가한다.
        [f]. 플레이어의 위치와 네비게이션을 속성으로 갖는다.
            public Transform target; NavMeshAgent nav;
        [g]. Update() 함수에서 타겟을 추적한다.
            nav.SetDestination(target.position);
        [h]. 스크립트의 속성을 채워준다.

    #3. 투사체(바위) 만들기
        [a]. Boss Rock에 리지드바디, 스피어 콜라이더 부착
            구를 예정이기 때문에 Mass를 높여 주고 Angular Drag를 없앤다.
            x축으로만 구를 예정이므로 Freeze Rotation y, z축을 잠근다.
        [b]. 스크립트 생성 BossRock
            리지드바디, 회전 파워, 크기, 공격 준비 플래그를 속성으로 갖는다.
                Rigidbody rigid; float angularPower = 2; float scaleValue = 0.1f; bool isShoot;
        [c]. 기를 모으는 코루틴 함수를 만든다. GainPowerTimer
            2.2초 대기 -> 발사 isShoot = true;
        [d]. 발사하는 코루틴 함수를 만든다. GainPower
            발사 준비되기 까지 계속 대기
                while(!isShoot)
                대기하면서 회전 속도와 크기를 점차적으로 증가 시킨다.
                    angularPower += 0.02f; scaleValue += 0.005f; transform.localScale = Vector3.one * scaleValue; rigid.AddTorque(transform.right * angularPower, ForceMode.Acceleration);
                    yield return null;
        [e]. Awake() 함수에서 두 개의 코루틴 함수를 호출한다.
        [f]. BossRock도 Bullet 클래스를 상속하도록 한다.
        [g]. BossRock에 EnemyBullet 태그와 레이어를 등록
        [h]. Bullet 스크립트에서 총알이 바닥에 충돌할 때 3초 뒤에 사라지는 로직을 만들어 두었다.
            BossRock이라는 플래그를 만든다. public bool isRock;
            !isRock을 제어문에 추가한다.
        [i]. BossRock에 부착된 스피어콜라이더를 복사하여 트리거 체크하여 벽이나 플레이어에 충돌할 때 사라지도록 한다.
        [j]. 미사일과 바위를 프리팹화 한다.
        [k]. 위치 초기화
    
    #4. 보스 로직 준비
        [a]. Boss 스크립트 역시 Enemy 클래스를 상속할 예정
            Enemy 스크립트에서 Type을 추가한다.
            Awack에서 플레이어 추적을 시작 했었는데 Type D는 추적하지 않도록 한다.
            Update에서 네비게이션 추적의 제어문으로 Type D는 추적하지 않도록 한다.
            Targeting 함수 로직또한 Type D는 하지 않는다.
            피격을 받아 제거되는 로직에서도 Type D는 제외한다.
        [b]. 몬스터가 피격 받을 때 색이 바뀌는데 이 때 Material 속성으로 하나의 Material만을 받아 왔었다.
            속성을 배열로 바꿔준다.
            foreach문으로 mesh.material.color 색을 바꾼다.
        [c]. Boss 스크립트를 만들고 Enemey D에 부착
            Enemy 클래스를 상속

    #5. 기본 로직
        [a]. 보스 미사일 프리팹, 미사일 발사 위치를 속성으로 갖는다.
            public GameObject missile; public Transform missilePortA, B;
            플레이어의 움직임을 통해 다음 플레이어의 위치를 저장할 벡터 속성을 갖는다.
                Vector3 lookVec; Vector3 tauntVec;
            보스가 내려찍기를 할 때 한 번 지정된 방향을 그대로 고정할 수 있도록 플래그 속성을 갖는다.
                bool isLook;
        [b]. Update 함수에서 플레이어를 바라보는 함수를 만든다.
            플레이어를 바라보는 것이 true일 때 Player 처럼 수직, 수평 값을 받아서 저장하고 lookVec에 저장한다.
                lookVec = new Vector3(h, 0, v) * 5f;
            보스가 지정된 방향을 바라본다.
                transform.LookAt(target.position + lookVec);
        [c]. Start 함수에서 isLook true;
        [d]. 씬으로 나가서 스크립트의 속성을 채워준다.

    #6. 패턴 로직
        [a]. Start 함수를 지우고 Awake 함수와 Think 코루틴 함수를 만든다.
            Awake에서 코루틴을 호출한다.
        [b]. 생각을 하기 위해 0.1초 정도 쉬고 공격 패턴을 랜덤으로 받는다.
            int ranAction = Random.Range(0, 5);
            switch문으로 3개의 패턴을 확률로 고려하여 나눈다.
                미사일은 0, 1
                바위 2, 3
                점프 4
        [c]. 코루틴으로 3개의 공격 패턴을 만들어 주고 swich문에서 호출한다.
        [d]. Enemy 스크립트에서 public으로 지정하지 않았던 속성을 public으로 바꿔준다.
            이때 Awake 함수로 초기화 하였던 속성들은 실행이 되지 않는다.
                생명 주기 이론 상으로 Awake 함수는 상속받은 자식 스크립트만 단독으로 실행된다.
                    네임스페이스 AI 추가
            그러므로 Boss 스크립트의 Awake() 함수에서 다시 초기화를 해준다.
        [e]. 공격 패턴 코루틴 함수에서 각각의 애니메이션 파라미터를 전달 한다.
            액션 하나당 걸리는 시간을 준다. 미사일 2, 바위 3, 점프 1
            액션이 끝나면 다시 생각을 한다.
        [f]. isLook을 public으로 지정하여 시작 부터 플레이어를 바라보도록 체크한다.
        [g]. MissileShot 코루틴 함수에서 애니메이션을 출력하고 0.2초 뒤에 한 발, 0.3초 뒤에 한 발을 발사
            GameObject instantMissileA = Instantitate(missile, missilePortA.position, ...)
            만들어진 미사일로 부터 스크립트를 받아와서 미사일 속성 target을 채워준다.
        [h]. RockShot 코루틴 함수에서 기를 모으는 동안 방향이 고정되어야 함으로 isLook false
            애니메이션이 실행 되고 바위를 인스턴스화
                Instantiate(bullet, transform.position, transform.rotation);
            발사 후에 true;
        [i]. Taunt 코루틴 함수에서 내려찍을 위치로 점프한다.
            tauntVec = target.position + lookVec;
            isLook = false;
            보스의 충돌체가 플레이어와 충돌하여 밀지 않도록 잠시 비활성화
            1.5초 뒤에 보스 발바닥 콜라이더를 활성화
                meleeArea.enabled = true;
            0.5초 뒤에 다시 발바닥 비활성화
            1초 뒤에 isLook true, 충돌체도 true;
        [j]. 보스는 네비게이션을 사용하지 않기 위해 Awake에서 정지 시킨다.
            nav.isStopped = true;
        [k]. Update에서 isLook이 false일 경우 착지 위치로 추적
            nav.SetDestination(tauntVec);
            Taunt 코루틴 함수에서 isLook을 false로 바꿀 떄 네비게이션의 멈춤도 해제한다.
                nav.isStopped = false;
                점프 공격이 끝나면 다시 true;
        [l]. Enemy 스크립트에 죽었다는 플래그 속성을 만든다. isDead;
        [m]. Enemy가 피격을 받아서 체력이 0 이하로 떨어질 때 isDead를 true로
        [n]. Targeting 함수에서 제어문을 추가한다. !isDead
            Boss 스크립트에서 Update 로직을 실행 하기 전에 isDead일 경우 모든 코루틴을 멈춘다.
                StopAllCoroutines();
                그리고 반환

    #7. 로직 점검
        [a]. Melee Area 오브젝트에 Bullet 스크립트 부착
        [b]. 미사일 후속타가 플레이어 무적 중에는 Destroy가 실행되지 않는다.
            Player 스크립트에서 무적 타임 제어문 밖에다가 EnemyBullet 태그 오브젝트를 제거하는 로직을 작성
        [c]. 보스가 점프공격을 하였을 때 충돌체가 다시 활성화 되는데 이 때 플레이어가 튕겨저 나간다.
            플레이어가 점프공격을 맞을 때 넉백이 되도록 한다.
            Melee Area 객체를 Boss Melee Area로 명명
        [d]. Player 스크립트에서 EnemyBullet 태그와 충돌 했을 때 지역 변수로 보스 공격인지 확인하고 Ondamage 함수의 매개 변수로 전달한다.
            bool isBossAtk = other.name == "Boss Melee Area";
        [e]. 만약 보스 공격을 받았다면 현재 플레이어 방향의 뒤로 힘을 가한다.
            if(isBossAtk)
                rigid.AddForce(transform.forward * -25, 임펄스);
            1초가 지난 뒤에 다시 velocity를 0으로
                if(isBossAtk)
                    rigid.velocity = Vector3.zero;
*/

/*
14. 3D 쿼터뷰 액션 게임 - UI 배치하기

    #1. 캔버스 세팅
        [a]. 캔버스 만들기
        [b]. Pixel Perfect 체크
        [c]. UI Scale Mode : Scale With Screen Size
        [d]. 1920 * 1080

    #2. 타이틀 메뉴 UI
        [a]. 캔버스의 자식으로 UI Panel 생성 Menu Panel
            Menu Panel의 자식으로 UI 이미지 생성 Title Image
                Title 스프라이트 적용, y축을 올림
            Menu Panel의 자식으로 UI 이미지 생성 MaxScore Image
                Icon Score 스프라이트 적용
            Menu Panel의 자식으로 UI 텍스트 생성 MaxScore Text
                9999999
            Menu Panel의 자식으로 UI 버튼 생성
                Panel A 스프라이트 적용
                버튼 텍스트 Game Start

    #3. 인게임 주요 UI
        [a]. 캔버스의 자식으로 UI Panel 생성 Game Panel
            알파값 제로
        [b]. GamePanel의 자식으로 빈 오브젝트를 만든다. Score Group
            Score Group의 자식으로 이미지와 텍스트를 만든다.
                Score 스프라이트 적용
            앵커 좌측 상단
        [c]. Score Group 복사 Status Group
            앵커 좌측 하단
            이미지와 텍스트를 두쌍 더 만든다.
            플레이어의 체력, 총알, 돈 스프라이트 적용
        [d]. Score Group 복사 Stage Group
            앵커 우측 상단
            스테이지 스프라이트 적용
            이미지와 텍스트 한 쌍 더 만든다.
            플레이 시간 스프라이트 적용
        [e]. Score Group 복사 Enemy Group
            앵커 우측 하단
            몬스터 타입 3가지 스프라이트 적용

    #4. 장비 UI
        [a]. Game Panel의 자식으로 빈 오브젝트를 만든다. Equip Group
            앵커를 아래로
        [b]. Equip Group의 자식으로 이미지 생성
            Panel B 스프라이트 적용
                앵커를 아래로
            이미지 자식으로 이미지 생성
                icon 망치 스프라이트 적용
                자식 이미지를 복사하고 icon 1 스프라이트 적용
                    앵커 좌측 상단
        [c]. 이미지를 4개 더 복사
            좌우로 위치 조정
            가운데 3개는 무기 3개, 가장 왼쪽은 왼쪽 클릭, 가장 오른쪽은 수류탄

    #5. 보스 체력 UI
        [a]. Game Panel의 자식으로 빈 오브젝트를 만든다. Boss Group 
        [b]. Boss Group의 자식으로 이미지 생성
            게이지 백 스프라이트 적용
            앵커 상단으로
            이미지의 자식으로 이미지 생성 Boss Health Image
                게이지 프론트 스프라이트 적용
            이미지의 자식으로 이미지 생성 Boss Image
                icon 보스 스프라이트 적용
                    앵커 좌측
            이미지의 자식으로 이미지 생성 Boss Text
                보스 텍스트 스프라이트 적용
        [c]. Boss Health Image의 shift 앵커 좌측
*/

/*
15. 3D 쿼터뷰 액션 게임 - UI 배치하기

    #1. 상점 꾸미기
        [a]. 빈 오브젝트 생성 Item Shop
            자식으로 3D Cube로 진열대 만들기, 박스 콜라이더 삭제 Table
        [b]. Material을 만들어서 _Pattern 재질 적용
            타일링 조절
        [c]. Item Shop 자식으로 총알, 체력, 수류탄을 나열
            Itemp Shop 자식으로 빈 오브젝트를 만들어서 아이템들을 자식으로 등록 Goods Group
        [d]. Item Shop의 자식으로 Luna 객체 등록
            Mesh Object에 애니메이터 컨트롤러 Luna 등록
                애니메이션 클립 Idle, Hello 등록
                    Any State에서 Hello 연결
                    트리거 doHello 생성
        [e]. Item Shop의 자식으로 빈 오브젝트 생성 Zone
            파티클 시스템 컴포넌트 부착
                쉐이프 Donut, Rotation x 90, 라디우스 2.5, 도너츠 라디우스 0.001, 아크 loop
                스타트 스피드 0
                스타트 사이즈 0.2
                이미션 60
                색에 그라데이션
            스피어 콜라이더 부착 트리거
                태그 Shop
        [f]. Shop 스크립트 생성 및 Zone에 부착
        [g]. Item Shop의 자식으로 빈 오브젝트 생성 Spawn Pos A, B, C
            적절한 위치 지정
        [h]. Item Shop 복사 Weapon Shop
            Luna 대신 Ludo 등록
            Ludo 애니메이터 만들고 MeshObject에 부착
                애니메이션 클립 Idle, Hello 등록
            무기 상품 나열

    #2. UI 구축하기
        [a]. Game Panel에 빈 오브젝트 만들기 Itemp Shop Group
            캔버스에서 벗어나도록 y축을 아래로 내린다. 
            사이즈 1000 500
            이미지 컴포넌트 추가
                Panel A 스프라이트 적용
                파티클 색상과 비슷하게 색 변경
        [b]. Itemp Shop Group의 자식으로 버튼 생성 Item Button A
            사이즈 240 380
            Panel A 스프라이트 적용
            Name Text, Price Text 생성
            Image 추가 Icon Heart 스프라이트 적용
            Image 추가 하여 동전 스프라이트 적용
        [c]. Item Button A를 복사 B, C
            총알, 수류탄 적용
        [d]. Itemp Shop Group의 자식으로 버튼 생성
            Icon Close 스프라이트 적용
        [e]. Itemp Shop Group의 자식으로 이미지 생성
            Icon Luna 스프라이트 적용
        [f]. Item Button A의 자식 Text를 복사하여 Item Shop Group 자식으로 두고 앵커를 아래 가득 채움
            Talk Text
        [g]. Item Shop Group을 복사하여 Weapon Shop Group으로 만들고 구성을 바꿔준다.

    #3. 상점 출입
        [a]. Shop 스크립트에서 UI그룹, 애니메이터, 플레이어를 담을 속성을 갖는다.
            public RectTransform uiGroup; public Animator anim; Player enterPlayer;
        [b]. 속성을 채운다.
        [c]. Enter() Exit() 함수 생성
        [d]. Enter()에서 player 속성을 채워 준다.
            enterPlayer = player;
            player을 받기 위해 매개 변수로 Player player
            플레이어가 들어 왔다면 UI를 띄운다.
                uiGroup.anchoredPosition = Vector3.zero;
        [e]. Exit()에서는 UI창을 다시 내린다.
            uiGroup.anchoredPosition = Vector3.down * 1000;
            그리고 플레이어가 떠날 때 애니메이션을 출력한다.
        [f]. Player 스크립트에서 OnTriggerStay에 nearObject 속성에 인접한 무기를 저장하였었다.
            제어문에 Weapon 이나 혹은 Shop으로 수정한다.
        [g]. Interaction() 상호 작용 함수에서 Weapon 태그 이외에 Shop 태그일 경우를 추가한다.
            Shop 스크립트를 받아오고 Enter 함수를 실행 시키면서 자신을 전달해 준다.
        [h]. 플레이어가 Zone을 나갈 때 OnTriggerExit Shop 태그일 경우 스크립트를 불러와서 Exit() 함수를 호출한다.
            Shop shop = nearObject.GetComponent<Shop>();
            shop.Exit();
            nearObject = null;
        [i]. Exit Button에 Zone의 Exit() 함수를 연결한다.

    #4. 아이템 구입
        [a]. Shop 스크립트에서 Buy 함수를 만든다.
            매개 변수로 인덱스값을 받는다.
        [b]. 아이템 오브젝트 배열과 아이템 가격 배열, 아이템 스폰 위치 배열, 대사 텍스트, 대사 데이터
            using UnityEngine.UI;
            public GameObject[] itemObj; public int[] itemPrice; public Transform[] itemPos; public Text talkText; public string[] talkData;
        [c]. 아이템과 무기 각각 속성을 채워준다.
        [d]. Buy 함수에서 지역 변수를 만들어서 가격을 저장한다.
            int price = itemPrice[index];
            만약 보유 가격보다 템 가격이 비싸면 이전 코루틴 정지, 그리고 코루틴 함수 호출, 그리고 반환
        [e]. 코루틴 Talk 함수 생성
            talkText.text = talkData[1];
            2초 뒤에 다시 기존 대사 출력
            talkText.text = talkData[0];
        [f]. Buy 함수에서 만약 돈이 충분하다면 보유한 돈을 차감하고 지정된 위치에 아이템 인스턴스화
            enterPlayer.coin -= price;
            Vector3 ranVec = Vector3.right * Random.Range(-3, 3) + Vector3.forward * Random.Range(-3, 3);
        [g]. UI 버튼에 Buy 함수를 등록한다.

    #5. 액션 제한
        [a]. 플레이어 스크립트에서 쇼핑 중임을 체크할 플래그 속성을 갖는다.
            bool isShop;
        [b]. 상호작용 함수에서 Shop 스크립트를 가져올 때 true;
        [c]. 나갈 때 false;
        [d]. Attack 제어문에 추가
*/

/*
16. 3D 쿼터뷰 액션 게임 - UI 로직 연결하기

    #1. 타이틀 카메라
        [a]. Main Camera를 Game Camera로 명명
        [b]. Game Camera를 복사하여 Menu Camera로 명명
        [c]. Game Camera를 비활성화 한다.
        [d]. Menu Panel 을 활성화 시키고 Game Panel과 Player를 비활성화 한다.
        [e]. Menu Camera의 follow 스크립트 제거
            y축과 z축을 조정하여 더 넓은 시야로 바라보도록 한다.
        [f]. 애니메이션을 만든다. Yoyo Rotation
            Menu Camera에 부착
            Yoyo 애니메이션에서 Rotation 프로퍼티 추가
                y축으로 회전을 준다.
                loop Time

    #2. 최고 점수 기록
        [a]. Player 스크립트로 가서 최고 점수를 저장한다.
        [b]. Awake() 함수에서 PlayerPrefs로 점수를 저장한다.
            PlayerPrefs.SetInt("MaxScore", 0);
        [c]. 플레이어를 활성화 한 상태에서 실행

    #3. 변수 세팅
        [a]. GameManager 만들기
        [b]. 게임 오브젝트 menuCam; gameCam;
        [c]. 스크립트 player; boss;
        [d]. 게임 정보 int stage; float playTime;
        [e]. 플래그 bool isBattle;
        [f]. 스테이지 정보 int enemyCntA,B,C;
        [g]. UI 게임 오브젝트 menuPanel; GamePanel;
            메뉴의 최대 점수 Text maxScoreTxt; Text scoreTxt; Text stageTxt; Text playTimeTxt;
            플레이어 정보 Text playerHealthTxt; Text playerAmmoTxt; Text playerCoinTxt;
        [h]. 이미지 weapon1Img 2,3,R;
        [i]. 적 정보 텍스트 enemyATxt B, C;
        [j]. UI위치 RectTransform bossHealthGroup;
            RectTrasnform bossHealthBar;

    #4. 게임 시작
        [a]. Menu Camera 세팅
        [b]. GameManger 스크립트에서 최고 점수를 출력한다.
            Awake()
                maxScoreTxt.text = string.Format(("{0:n0}", PlayerPrefs.GetInt("MaxScore"));
        [c]. 게임 시작 버튼과 연동될 함수 만들기
            GameStart(), 활성화, 비활성화
                menuCam, menuPanel 비활성화
                gameCam, gamePanel, 플레이어 오브젝트 활성화
        [d]. 버튼에 GameStart함수 등록

    #5. 인게임 UI
        [a]. Game Camera 세팅
        [b]. GameManager 스크립트에서 게임 정보를 출력 LateUpdate()
            현재 점수, 체력, 돈의 수치를 표시
                scoreTxt.text = string.Format("{0:n0}", player.score);
                playerHealthTxt.text = string.Format(player.health + " / " + player.maxHealth);
                playerCoinTxt.text = string.Format("{0:n0}", player.coin);
            플레이어무기 상태에 따라 탄약 표시
                player.equipWeapon == null || player.equipWeapon.type == Weapon.Type.Mell
                else
                    playerAmmoTxt.text = player.equipWeapon.curAmmo + " / " + player.ammo;
            무기 UI의 색을 보유 여부에 따라 조정한다.
                weapon1Img.color = new Color(1, 1, 1, player.hasWeapons[0] ? 1 : 0);
                    1,2,3
                    수류탄 R의 경우 0보다 큰지
            각 몬스터의 수
                enemyATxt.text = enemyCntA.ToString();
                    A, B, C
            스테이지 번호
                stageTxt.text = "STAGE " + stage;
            플레이 타임
                int hour = (int)(playTime / 3600);
                int min = (int)((playTime - hour * 3600) / 60);
                int second = (int)(playTime % 60);
                playTimeTxt.text = string.Format("{0:00}", hour) + ":" + string.Format("{0:00}", min) ":" + string.Format("{0:00}", second);
            보스의 체력
                bossHealthBar.localScale = new Vector3((float)boss.curHealth / boss.maxHealth, 1, 1);
        [c]. Player 스크립트에 현재 점수 속성을 추가 public int score;
            equipWeapon 속성을 public으로 수정
        [d]. Update 함수에서 전투 중일 때 시간을 짆행 시킨다.
            if(isBattle)
                playTime += Time.deltaTime;
*/

/*
17. 3D 쿼터뷰 액션 게임 - 게임 완성하기

    #1. 스테이지 관리
        [a]. 게임 매니저 스크립트에 스테이지 관련 함수를 만든다.
            StageStart() StageEnd()
        [b]. 씬에서 Zone을 복사하여 스테이지 시작 Zone으로 만든다. Start Zone
            스크립트 제거, 태그 제거
            Start Zone의 자식으로 3D 텍스트를 만든다. Next Stage
        [c]. StartZone 스크립트 생성 및 부착
        [d]. StartZone은 게임 매니저 속성을 받아서 플레이어와의 충돌에서 스테이지를 시작한다.
            public GameManager manager;
            OnTriggerEnter
                태그가 Player일 경우 게임 매니저의 StageStart() 함수 호출
        [e]. 다시 게임 매니저에서 코루틴 함수 InBattle()을 만든다.
            5초간 쉬었다가 StageEnd() 호출
            StageStart함수에서 isBattle true로, 코루틴 호출
            StageEnd는 false
        [f]. 전투가 시작될 때 샵과 시작 존을 비활성화 하기 위해 속성으로 갖는다.
            public GameObject itemShop; public GameObject weaponShop; public GameObject startZone;
        [g]. StageStart에서 비활성화
            StageEnd()에서 활성화 그리고 stage++; 그리고 플레이어 위치를 원위치로
                player.transform.position = Vector3.up * 0.8f;

    #2. 몬스터 프리팹
        [a]. Enemy 스크립트에 점수, 동전 프리팹 배열을 속성으로 만든다.
            public int score; public GameObject[] coins;
        [b]. 몬스터가 피격을 받아 죽을 때 플레이어 스크립트를 받아와 자신의 점수를 넘겨 준다.
            Player player = target.GetComponent<Player>();
            player.score += score;
            랜덤한 값을 받아서 랜덤한 코인을 만든다.
                int ranCoin = 랜덤
                Instantiate(coins[ranCoin], 위치, Quaternion.identity);
            Boss도 걍 죽임
        [c]. 씬에서 몬스터 별로 자신의 점수를 채운다.
        [d]. 위치값 초기화 한 뒤 프리팹화 한다.

    #3. 몬스터 관리
        [a]. Start Zone을 4개 복사한다. 콜라이더, 스크립트 제거 Enemy Respawn Zone
            동서남북으로 위치시킨다.
            Enemy Zone Group에 넣어 준다.
        [b]. 게임 매니저에 Transform[] 속성을 만든다. enemyZones;
            몬스터 프리팹 배열도 만든다. enemies;
            스테이지에 출현하는 몬스터 리스트 public List<int> enemyList;
        [c]. Awake() 에서 리스트 초기화
            enemyList = new List<int>();
        [d]. InBattle 함수에서 리스트를 채워 준다.
            반복문으로 현재 stage의 값 *2 만큼 반복하면서 랜덤값을 만들고 리스트에 랜덤값을 저장한다.
                enemyList.Add(ran);
            그리고 바로 소환 
                while(enemyList.Count > 0)
                    int ranZone = 랜덤
                    GameObject instantEnemy = Instantiate(enemies[enemyList[0]], enemyZones[ranZone].position, ...);
                    Enemy enemy = instantitate.GetComponent<Enemy>();
                    enemy.target = player.transform;
                    enemyList.RemoveAt(0);
                    yield 4초
            StageEnd() 호출 삭제
        [e]. LateUpdate에서 보스의 체력바를 띄우는 로직에 boss가 null이 아닐 때만 표시하도록 한다.
        [f]. Enemy Respawn Zone을 비활성화 시켜 놓는다.
        [g]. StageStart 함수에서 enemyZones를 순회하며 소환 존을 활성화 한다.
            foreach(Transform zone in enemyZones)
        [h]. StageEnd 함수에서는 반대로 enemyZones를 순회하며 비활성화
        [i]. 몬스터 수를 출력하는 UI를 위해 리스트에 몬스터 값을 채울 때 수를 증가 시켜준다.
            switch(ran)
                enemyCntA++;...
        [j]. 5단위의 스테이지 번호 마다 보스를 소환한다. 아닐 때는 일반 몹
            if(stage % 5 == 0)
                몬스터 처럼 보스 소환
                    GameObject instantEnemy = Instantiate(enemies[3]...)
                    ...
                    boss = instantEmeny.GetCompoenent<Boss>();
        [k]. 몬스터가 다 출력되고 남은 몬스터를 확인하는 반복문을 만든다.
            while(enemyCntA + enemyCntB... > 0)
                yield ...
            반복문 탈출 뒤에 4초 쉬었다가 StageEnd함수 호출
        [l]. Enemy 스크립트에 게임 매니저 속성을 만든다.
            몬스터가 죽을 때 switch문으로 자신의 타입을 토대로 게임 매니저의 몬스터 수를 차감한다.
                manager.enemyCntA--;...
        [m]. 게임 매니저에서 몬스터를 소환할 때 자기 자신을 Enemy 스크립트의 매니저 속성으로 보낸다.
        [n]. 게임 매니저에서 보스를 잡고 나면 체력바를 사라지게 한다.
            LateUpdate에서 체력바를 표시하는 로직에 보스가 소환될 때 앵커 위치를 내려주고 아니면 올려준다.
                bossHealthGroup.anchoredPosition = Vector3.down * 30;
                ...
        [o]. Boss는 잡히고 나면 다시 null이 되어야 하므로 스테이지 종료를 호출하기 바로 전에 null을 배정한다.

    #4. 게임 오버
        [a]. 게임 매니저에 게임 오버 함수 생성
        [b]. 플레이어 스크립트에서 OnDamage 코루틴 함수로 간다.
            if(health <= 0) 제어문을 만들고 OnDie() 함수를 호출한다.
        [c]. 플레이어 스크립트에 OnDie() 함수를 만든다.
            죽음 애니메이션을 출력한다.
            isDead라는 속성을 만든다. 그리고 true;
            각 각의 액션에 제어문을 추가한다.
        [d]. 플레이어 애니메이션 클립으로 Die를 등록 doDie 트리거 생성
        [e]. 플레이어 스크립트에 속성으로 게임 매니저를 만든다.
            OnDie에서 게임 매니저의 GameOver() 함수를 호출한다.
        [f]. 캔버스의 자식 중 Menu Panel을 복사한다. GameOver Panel
            Title은 지운다.
            버튼 텍스트는 Main Title로 지정
            새로운 텍스트를 만들고 Best라 입력, 만약 현재 점수가 최고 점수를 넘겼을 때 띄움
                일단 비활성화
            새로운 텍스트 만들고 GAME OVER 작성
        [g]. 게임 매니저에서 속성으로 게임 오버 판넬을 받아온다.
            Text로 게임 오버 판넬에 띄울 최종 점수와 최고점수 확인 속성을 추가
                public Text curScoreTxt; public Text bestTxt;
        [h]. 게임 오버 함수에서 게임오버 판넬을 활성화 하고 게임 판넬을 비활성화
            현재 점수를 출력
                curScoreTxt.text = scoreTxt.text;
            최고점수를 받아와서 비교
                int maxScore = PlayerPrefs.GetInt("MaxScore");
                if(player.score > maxScore)
                    bestText.gameObject.활성화
                    PlayerPrefs의 최고 점수 저장
        [i]. Restart() 함수 생성
            씬을 로드한다.
*/

/*
1. Spwan 스크립트 생성
    상속 없음
2. 속성 : 시간, 타입, 지점
3. 메모장에 순서대로 기입 구분좌는 한 종류로 통일하고 한마리 스폰은 스페이스바로 구분
4. Resources 폴더를 에셋에 저장
    폴더 안에 메모장 파일 저장
5. 게임 매니저에서 파일을 읽는다.
    속성으로 Spawn 구조체 속성의 리스트와 리스트의 인덱스, 모두 읽었는지 확인할 플래그 필요
    파일을 읽는 함수를 만든다.
6. 우선 리스트를 초기화 한다.
7. 파일을 읽는 함수에서 읽기 전에 리스트에 저장된 이전 목록을 깔끔하게 지워준다.
    인덱스와 플래그를 함께 초기화 한다.
8. 파일을 읽기 위한 라이브러리를 추가한다.
9. TextAsset(텍스트 파일 에셋 클래스) 타입의 변수를 만들고 메모장을 저장한 Resources 폴더를 통해 메모장을 Load 한다.
10. StringReader(파일 내의 문자열 데이터 읽기 클래스) 타입의 변수를 만들고 텍스트 파일의 텍스트를 불러 온다.
11. ReadLine() 메서드로 텍스트 데이터를 한 줄씩 읽는다.
12. Spawn 구조체로 값을 저장한다.
    Split으로 가르고 Parse로 타입에 맞게 변환해 준다.
13. 구조체를 리스트에 저장
14. 반복문을 만들어서 한 줄을 읽고 제어문으로 읽은 라인이 null인지 확인한다. 아니면 리스트에 저장하는 작업을 수행하고 아니면 반복문을 탈출한다.
15. 다 읽은 뒤에는 파일을 닫아 준다.
*/





/* JSON */

/*
JSON( JavaScript Object Notation ) : 데이터 교환을 위한 경량의 데이터 형식
    => 텍스트 형태로 구성되며, 사람이 읽고 쓰기 쉽고 기계가 파싱하고 생성하기도 쉽게 변환
    => 주로 웹 어플리케이션과 서버 간의 데이터 교환에 사용되며, 
        => 배열과 객체( 키-값 쌍 )의 조합으로 구성된다. 
    => JSON은 프로그래밍 언어나 플랫폼에 종속되지 않고 일반적으로 사용될 수 있다.



1. Json Data 로 변환할 구조체 혹은 클래스 생성 ( TestData : 객체의 속성 집합 )
2. ToJson 혹은 FromJson 을 통해 데이터를 읽고 쓸 클래스 만들기 ( TestUserData )
3. Data 를 받기 위한 속성을 만들어 준다.
4. Start() 함수에서 string 타입 지역 변수를 만들어서 Json 으로 변환된 Data 를 저장 받는다.
    => string jsonData = JasonUtility.ToJson(testPlayer);
    => print(jsonData);
5. Start() 함수에서 Data 를 받기 위한 지역 변수를 만들어서 C# Data 로 변환된 Data 를 저장 받는다.
    => TestData testResive = JsonUtility.FromJson<TestData>(jsonData);
    => print(testResive.name);
*/

/*
정보 조회 프로그램( Json + HTTP + API )
    => 던전앤파이터의 유저 정보 검색 프로그램을 만들어 보자

1. 던전앤파이터 API 홈페이지에 접속하여 "서버 URL"을 받아온다.
2. 서버 선택 + 캐릭터 검색을 담당할 스크립트를 만든다.( TestServer )
    => UI 와의 상호 작용, 서버와의 통신을 위한 네임스페이스를 추가 한다.
        => using UnityEngine.UI;
        => using UnityEngine.Networking;
3. 서버와의 통신을 위한 코루틴 함수를 만든다.( ServerRequiest )
    => UnityWebRequest : Unity 엔진에서 제공하는 네트워크 요청 및 통신을 관리하는 클래스
    {
        1. 서버 URL 을 string 타입의 지역 변수로 받는다.
        2. UnityWebRequest 타입의 지역 변수를 만들고 Getter 를 통해 주소를 저장 한다.
            => string url = URL 주소
            => UnityWebRequest www = UnityWebRequest.Get(url);
        3. 웹 서버로 요청을 보내고 요쳥이 완료될 때 까지 대기
            => yield return www.SendWebRequest();
        4. 에러가 발생하지 않았다면 웹 서버로부터 받은 Json 데이터를 문자열로 출력
            => if(www.error == null) Debug.Log(www.downloadHandler.text);
        5. 에러가 있다면 에러를 출력
            => else Debug.Log("Server Load Error");
    }
4. Start() 함수에서 서버 통신 코루틴을 호출한다.
    => Response Body : 
        클라이언트가 서버에게 보낸 요청( Request )에 대한 서버의 응답( Response ) 중에서, 
        실제 데이터가 담겨 있는 부분을 말한다. 
        이 부분은 주로 JSON, XML 등의 형식으로 구성되어 클라이언트에게 반환된다.
    => 출력된 Response Body를 복사 한다.
5. 구글 검색 : Json to C# -> Convert JSON to C# .... 선택
    => Convert -> Convert 내용을 복사하여 스크립트에 등록( ServerInfo, ServerRoot )
    => 클래스 위에 [System.Serializable] 작성하여 JSON이나 XML과 같은 형식으로 직렬화하여 사용
6. 서버 통신 코루틴으로 돌아가서 Json Data를 C# Data로 변환하여 받는다.
    {
        ServerRequiest 코루틴... 요청을 받은 다음...
        
        1. JsonUtility 클래스를 사용하여 Json 타입의 문자열을 C# Data( ServerRoot ) 클래스의 객체로 역직렬화 한다.
        var serverData = JsonUtility.FromJson<ServerRoot>(www.downloadHandler.text);
        2. Json Data 를 담은 리스트에 접근해 서버이름을 출력해 본다.
        print(serverData.rows[0].serverName);
    }
7. Dropdown 의 항목에 서버에서 받은 Data 를 넣기 위해 먼저 속성을 만든다.( public Dropdown serverList; )
8. 서버 통신 코루틴으로 돌아가서 서버 이름을 하나씩 집어 넣는다.
    {
        1. foreach() 문으로 Data 리스트에 하나씩 접근하여 서버 이름을 받는다.
        foreach(var sd in serverData.rows)
        2. 드롭다운은 옵션을 통해서 스크롤 항목이 늘어나기 때문에 옵션 타입을 새롭게 만든다.
        Dropdown.OptionData option = new Dropdown.OptionsData();
        3. 옵션에 서버 이름을 배정한다.
        option.text = sd.serverName;
        4. 드롭다운 속성에 접근하여 options에 이번에 만든 옵션 타입 변수를 추가한다.
        serverList.options.Add(option);
    }

#. 선택된 서버를 인식하고 검색한 유저를 찾기 위해 Dropdown의 자식, Label와 InputField의 Text를 넘겨주도록 한다.
    
9. 속성으로 서버 아이디와 캐릭터 닉네임을 만든다.
    => string serverId = "";
    => string characterName = "";
10. 만들어둔 검색 버튼과 연동할 함수를 만든다. CharacterSearch()
    => 버튼이 눌리면 서버 아이디와 닉네임을 속성에 채워주면 된다.
11. 서버를 한글로 입력 받으면 이것을 영어로 변환해야 한다.
    => 코루틴에서 만들어 두었던 ServerRoot 지역 변수를 속성으로 만들어 준다.
        => ServerRoot serverData = new ServerRoot();
    => 선택된 서버를 알기 위해 Lavel의 텍스트를 속성으로 받는다.
        => public Text selectedServerName;
    {
        CharacterSearch() 함수

        1. 선택된 서버 이름을 임시 변수를 만들어서 저장한다.
        string temp = selectedServerName.text;
        2. 요청 받은 서버 Data 의 리스트에서 선택된 서버 이름과 같은 Data 를 찾아서 Id 속성에 저장한다.
        serverId = serverData.rows.Find(x => x.serverName == temp).serverID;
        print(serverId);
    }
12. 사용자가 검색한 닉네임이 저장된 InputField를 속성으로 받는다.
    => public InputField inputText;
13. 캐릭터 닉네임은 inputText.text를 배정
    {
        CharacterSearch() 함수...

        characterName = inputText.text;
    }
14. 캐릭터 검색 URL을 복사해서 받아 온다.
15. 코루틴 함수 추가 CharacterRequest(string serverId, string characterName)
    {
        1. URL 주소에서 필요한 정보만 받아 온다.
            => URL 주소 중 사용하지 않는 캐릭터 이름 뒤부터 apikey 전까지 지워준다.
        string url = $"URL 주소 {serverId}...{characterName}"
        2. 서버 선택 때와 마찬가지로 요청을 하고 요청을 돌려 받을 때 까지 대기한다.
        UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();
        3. 에러가 발생하지 않았다면 웹 서버로부터 받은 Json 데이터를 문자열로 출력
        if(www.error == nullptr)
            Print(www.downloadHandler.text);
        else
            Debug.LogError(www.error);
    }
16. 검색 버튼이 눌릴 때 호출되는 함수에서 코루틴을 호출한다.
17. URL 인코딩 작업을 거져서 CharacterName을 전달
    => URL 인코딩(URL Encoding) : URL에 사용되는 문자 중에서 특별한 의미를 가지거나 
        => URL 구조를 깨뜨릴 수 있는 문자들을 안전하게 전송하기 위해 해당 문자를 
        => 다른 형식으로 변환하는 과정
            => 예를 들어 공백 문자는 URL에서 %20으로 인코딩된다. 
                => 따라서 "Hello World"라는 문장을 URL에 포함하려면 
                => "Hello%20World"로 인코딩된다.
                => 이렇게 하면 웹 서버나 브라우저가 URL을 올바르게 해석할 수 있게 된다.
    => 검색 함수로 돌아가서 입력 받은 텍스트를 URL 형식으로 이스케이프하여 변환
    {
        CharacterSearch()...

        characterName = UnityWebRequest.EscapeURL(inputText.text);
    }

*. 검색을 통해 캐릭터 정보를 Json으로 받았다면 마찾가지로 JsonToC#으로 컨버트하여 정보를 표로 만들 수 있다.
    
18. 클래스가 많아지기는 하지만 일단 복사 CharacterInfo, CharacterRoot 명명
19. CharacterRoot 속성 선언 CharacterRoot characterData = new CharacterRoot();
20. 검색 코루틴으로 돌아가서 Json 파일을 캐릭터 Data 에 저장한다.
    {
        CharacterRequest(string serverId, string characterName)...

        characterData = JsonUtility.FromJson<CharacterRoot>(www.downloadHandler.text);
    }
21. 속성으로 캐릭터id를 선언한다. string characterId = "";
22. 다시 검색 코루틴으로 가서 서버를 받을때와 같이 리스트에서 캐릭터 이름을 찾아서 id를 배정한다.
    {
        CharacterRequest(string serverId, string characterName)...

        characterId = characterData.rows.Find(x => x.characterName == inputText.text).characterId;
    }
23. 캐릭터 이미지를 띄우는 코루틴 함수를 만든다. CharacterImageRequest(string serverId, string characterId)
    {
        1. 다른 코루틴과 마찬가지로 주소를 받고 요청하고 대기하고
        string url = $"URL 주소 {serverId}...{characterId}"( zoom 은 1로 고정 )
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();
    }
24. 씬에 만들어 둔 UI 이미지를 속성으로 받는다.
    public RawImage img;
25. 이미지 코루틴 함수로 돌아가서 변환 작업을 거친후 UI 속성에 저장
    {
        img.texture = ((DownloadHandlerTexture)www.downloadHandler).textrue;
    }
*/





/*
< Json 을 이용하여 저장/불러오기 기능 구현 >

1. 슬롯 선택 씬과 게임 진행 씬을 구분한다.
2. 각 씬의 기능을 담당해 줄 스크립트( Select, Game )를 만든다.
3. 데이터를 전담해서 만들어줄 스크립트( DataManager )를 만든다.
    {
        저장하는 방법
            1. 저장할 데이터가 존재해야 한다.
            2. 데이터를 Json 으로 변환
            3. Json 을 외부에 저장

        불러오는 방법
            1. 외부에 저장된 Json 을 가져온다.
            2. Json 을 데이터로 변환
            3. 불러온 데이터를 사용
    }
*/

/*
1. DataManager 클래스를 싱글톤 패턴으로 구성한다.
    public static DataManager instance;
    Awake()
    {
        if(instance == null) instance = this;
        else if(instance != this) Destroy(instance.gameObject);
        DontDestroyOnLoad(this.gameObject);
    }
2. Data 를 전담하여 관리해 줄 클래스를 만든다.
    public class PlayerData
    {
        public string name;
        public int stage = 1;
        public int score;
        public bool[] hasWeapon = new bool[3];
        public int ammo;
        public int grenade;
        public int coin = 1000;
    }
3. DataManager 클래스에서 Data 를 속성으로 만든다.
    PlayerData curPlayer = new PlayerData();
4. Start() 함수에서 Data 를 Json 으로 변환 한다.
    {
        1. 지역 변수에 Json 으로 변환된 Data 를 저장한다.
        string data = JsonUtility.ToJson(curPlayer);
        2. Json 을 출력한다.
        print(data);
    }
5. 저장/불러오기 를 하기 위해 네임스페이스를 추가
    using System.IO;
6. 저장 위치를 담을 속성, 파일 명 속성을 만든다.
    string path;
    string filename = save;
    => Awake() 함수로 가서 유니티가 지정해준 경로를 저장한다.
        =>path = Application.persistentDataPath + "/";
7. 다시 Start() 함수로 돌아가 File 클래스를 통해서 저장
    {
        Start()...

        File.WriteAllText(path + filename, data);
    }
8. 저장/불러오기 함수를 각각 만든다.
    public void SaveData(), public void LoadData()
9. Start() 함수에 작성하였던 저장 로직을 저장 함수로 옮긴다.
10. Load() 함수에서 저장된 경로에서 파일을 불러온다.
    {
        1. 지역 변수에 불러온 Json 을 저장
        string data = File.ReadAllText(path + filename);
        2. Json 을 Data 형식으로 변환 하여 현재 플레이어에 적용
        curPlayer = JsonUtility.FromJson<PlayerData>(data);
    }
*/

/*
1. Select 클래스에서 선택 화면 UI를 관리하기 위해 네이스페이스를 추가
    using UnityEngine.UI;
2. 빈 슬롯을 선택 했을 때 띄워지는 윈도우를 게임 오브젝트 속성으로 받는다.
    public GameObject creat;
3. 빈 슬롯에서 호출할 함수를 만든다.
    public void CreatSlot()
    {
        1. 생성 오브젝트를 활성화
        creat.SetActive(true);
    }
4. 슬롯의 텍스트 문구를 지정하기 위한 텍스트 속성을 만든다.
    public Text[] slotText;
5. 슬롯이 선택되었을 때 호출되는 함수를 만든다.
    public void Slot()
    {
        1. 저장된 데이터가 없을 때 생성 함수 호출
        CreatSlot();
    }
6. 게임 씬을 불러오기 위에 네임스페이스 추가
    using UnityEngine.SceneManagement;
7. 게임 씬을 불러오는 함수를 만든다.
    public void GoGame()
    {
        1. 씬을 호출한다.
        SceneManager.LoadScene(1);
    }
8. DataManager 클래스로 가서 현재 선택된 슬롯을 구분하기 위한 속성을 만든다.
    public int curSlot;
9. DataManager 클래스의 속성 curPlayer 를 public 으로 변환
10. DataManager 클래스에서 데이터를 저장할 때 슬롯 번호를 추가로 기입해 준다.
    {
        SaveData()...

        File.WriteAllText(path + filename + curSlot.ToString(), data);
    }
11. DataManager 클래스에서 데이터를 불러올 때도 슬롯 번호를 추가로 기입해 준다.
    {
        LoadData()...

        string data = File.ReadAllText(path + filename + curSlot.ToString());
    }
12. 다시 Select 클래스로 돌아가서 Slot 함수의 매개 변수로 슬롯 번호를 받는다.
    public void Slot(int slotNum)
    {
        1. DataManager 클래스의 속성 curSlot 에 매개변수를 배정
        DataManager.instance.curSlot = slotNum;

        CreatSlot();

        2. 저장된 데이터가 있을 때 DataManager 클래스의 데이터 불러오기 함수를 호출
        DataManager.instance.LoadData();
        3. 게임 씬을 불러오는 함수 호출
        GoGame();
    }
13. 새로운 슬롯을 만들 때 입력 받은 사용자 이름을 속성으로 받는다.
    public Text newPlayerName;
14. 네임스페이스로 저장/불러오기를 추가한다.
    using System.IO;
15. 새로운 슬롯을 만들 때 입력 받은 사용자 이름을 저장하기 위해 Start() 함수에서 저장된 데이터가 존재하는지 판단하도록 한다.
    Start()
    {
        1. 슬롯별로 저장된 데이터가 존재하는지 판단
        for(int i = 0; i < 4; i++)
            if(File.Exists(DataManager.instance.path + $"{i}"))
    }
16. DataManager 클래스에서 path 와 filename 으로 데이터를 저장하였는데, path 와 filename 을 합치도록 한다.
    => filename 속성을 지운다.
    => Awake() 에서 path 를 초기화 할 때 filename 을 그냥 리터럴로 추가하도록 한다.
    {
        Awake()...

        path = Application.persistentDataPath = "/save";
    }
    => 이후 path 속성을 public 으로 수정한다.
17. 다시 Select 클래스로 돌아가서 slot 에 저장된 파일이 존재하는지 저장할 bool 속성을 만든다.
    bool[] savefile = new bool[4];
18. Start() 함수에서 세이브 파일이 존재하는지를 속성에 저장
    {
        Start()...

        1. 데이터가 존재할 경우 참을 저장
        savefile[i] = true;
        2. 현재 슬롯 번호를 저장
        DataManager.instance.curSlot = i;
        3. 데이터 불러오기
        DataManager.instance.LoadData();
        4. 슬롯 텍스트를 사용자 이름으로 출력
        slotText[i].text = DataManager.instance.curPlayer.name;
        5. 만약 슬롯이 비어 있다면 비어있음 출력
        else slotText[i].text = "비어있음";
    }
19. DataManager 클래스에 slot 의 저장 정보를 담을 공간의 초기화 기능을 함수로 만든다.
    public void DataClear()
    {
        1. 슬롯의 번호를 초기화
        curSlot = -1;
        2. 사용자 Data 공간 초기화
        curPlayer = new PlayerData();
    }
20. Start() 함수에서 세이브 파일 존재 여부를 확인하였다면 정보 공간을 리셋
    {
        Start()...

        1. slot 저장 정보 초기화
        DataManager.instance.DataClear();
    }
21. Slot() 함수로 가서 슬롯 저장 상태에 따른 제어문을 추가 한다.
    {
        ...

        1. 저장된 데이터가 있다면 데이터를 불러온다.
        if(savefile[slotNum]) DataManager.instance.LoadData();
            2. 저장된 데이터가 있을 때 게임 화면으로 이동한다.
            GoGame();
        3. 자장된 데이터가 없다면 데이터를 만든다.
        else CreatSlot();
    }
22. GoGame() 함수로 가서 슬롯 저장 상태에 따른 제어문을 추가 한다.
    {
        1. 저장된 정보가 없다면 새로운 정보를 저장
        if(!savefile[DataManager.instance.curSlot])
            DataManager.instance.curPlayer.name = newPlayerName.text;
            DataManager.instance.SaveData();
    }
*/

/*
1. GameManager 클래스에서 스테이지가 종료될 때 자동 저장 시킨다.
    public void StageEnd()
    {
        ...

        1. DataManager 에서 Data 를 저장
        DataManager.instance.SaveData();
        2. 저장을 했다는 것을 유저에게 알림
        NoticeSave();
    }
2. 로드 되었다면 GameManager 속성에 점수, 돈, 총알 등의 값을 배정한다.
    {
        Start()...

        1. 스테이지 레벨 배정
        stage = DataManager.instance.curPlayer.stage;
        2. 점수 배정
        score = DataManager.instance.curPlayer.score;
        3. 플레이어가 보유하고 있는 무기
        for(int i = 0; i < player.hasWeapon.Length; i++)
            player.hasWeapon[i] = DataManager.instance.curPlayer.hasWeapon[i];
        4. 총알 개수 배정
        player.ammo = DataManager.instance.curPlayer.ammo;
        5. 수류탄 개수 배정
        player.grenade = DataManager.instance.curPlayer.grenade;
        6. 코인 배정
        player.coin = DataManager.instance.curPlayer.coin;
    }
*/