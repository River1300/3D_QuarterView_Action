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
        [d]. 피직스 매터리얼을 추가하여 모든 저항 갑을 1로 지정하여 콜라이더에 부착한다.
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
            OnDamage함수의 매개 변수로 수류탄인지를 반든다. bool isGrenade
            죽는 로직에서 수류탄인지 제어문을 만든다.
                수류탄일 경우 Vector3.up을 더 증가 시킨다.
                회전을 주기 위해 몬스터의 freezeRotation = false로 바꿔준다.
                rigid.AddTorque(reactVec * 15, 임펄스);
        [f]. 다시 수류탄 스크립트로 가서 반복문을 탈출 한 뒤 5초뒤 수류탄을 제거한다.
*/