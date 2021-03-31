using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lofle;
using System;

namespace Lofle
{
	public abstract class BaseState<MACHINE, BASE_STATE_TYPE>
		where MACHINE : BaseStateMachine<MACHINE, BASE_STATE_TYPE>
		where BASE_STATE_TYPE : BaseState<MACHINE, BASE_STATE_TYPE>
	{
		public class Permission
		{
			static protected void SetOwnerStateMachine( BASE_STATE_TYPE state, MACHINE machine ) { state.SetOwnerStateMachine( machine ); }
			static protected void Ready( BASE_STATE_TYPE state ) { state.Ready(); }
			static protected IEnumerator Coroutine( BASE_STATE_TYPE state ) { return state.Coroutine(); }
		}

		private bool _bActive = false;
		private MACHINE _ownerStateMachine = default( MACHINE );

		public bool isActive { get { return _bActive; } }

		/// <summary>
		/// 현재 상태를 제어중인 상태머신, 
		/// </summary>
		public MACHINE OwnerStateMachine { get { return _ownerStateMachine; } }

		/// <summary>
		/// 상태 종료
		/// </summary>
		public void Stop() { _bActive = false; }
		
		/// <summary>
		/// 상태 진입 시 최초로 호출
		/// </summary>
		virtual protected void Begin() { }

		/// <summary>	
		/// Begin 다음으로 호출
		/// </summary>
		/// <returns></returns>
		virtual protected IEnumerator Enter() { yield break; }

		/// <summary>
		/// 상태 유지시 지속적인 호출
		/// </summary>
		virtual protected void Update() { }

       
        /// <summary>
        /// 상태가 끝나는 경우 자동으로 호출
        /// </summary>
        virtual protected void End() { }

		/// <summary>
		/// 상태 전환
		/// </summary>
		/// <typeparam name="STATE">전환 상태</typeparam>
		/// <returns>다음 상태에 전달 값 처리용도</returns>
		protected STATE Invoke<STATE>()
			where STATE : BASE_STATE_TYPE, new()
		{
			return OwnerStateMachine.Change<STATE>();
		}

		private void SetOwnerStateMachine( MACHINE machine ) { _ownerStateMachine = machine; }

		private void Ready() { _bActive = true; }
		
		private IEnumerator Coroutine()
		{
			Begin();
			yield return Enter();
			while( _bActive )
			{
				yield return null;
				Update();
			}
			End();
		}
	}

	/// <summary>
	/// Owner 바로가기가 필요 없는 상태 타입
	/// </summary>
	public abstract class State : BaseState<StateMachine, State> { }

	/// <summary>
	/// Owner 바로가기 기능이 추가된 상태 타입
	/// </summary>
	public abstract class State<OWNER> : BaseState<StateMachine<OWNER>, State<OWNER>>
	{
		private OWNER _owner = default( OWNER );

		public OWNER Owner { get { return _owner; } set { _owner = value; } }
	}
}
