using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lofle;
using System;

namespace Lofle
{
	public class BaseStateMachine<MACHINE, BASE_STATE_TYPE> : BaseState<MACHINE, BASE_STATE_TYPE>.Permission
		where MACHINE : BaseStateMachine<MACHINE, BASE_STATE_TYPE>
		where BASE_STATE_TYPE : BaseState<MACHINE, BASE_STATE_TYPE>
	{
		private Type _currentStateType = null;
		private BASE_STATE_TYPE _currentState = null;
		private Dictionary<Type, BASE_STATE_TYPE> _stateDic = new Dictionary<Type, BASE_STATE_TYPE>();
		private Coroutine _coroutine = null;

		protected BASE_STATE_TYPE CurrentState { get { return _currentState; } }

		/// <summary>
		/// 상태 전환
		/// </summary>
		public STATE Change<STATE>() 
			where STATE : BASE_STATE_TYPE, new()
		{
			StopState( _currentStateType );
			return ChangeState<STATE>();
		}

		/// <summary>
		/// 상태머신의 라이프 사이클 처리,
		/// 특정 MonoBehaviour의 StartCoroutine를 사용이 필요 시 해당 함수 호출
		/// </summary>
		public IEnumerator Coroutine<STATE>() 
			where STATE : BASE_STATE_TYPE, new()
		{
			ChangeState<STATE>();

			do
			{
				yield return Coroutine( _currentState );
			}
			while( CurrentState != null && CurrentState.isActive );
		}

		/// <summary>
		/// Runner의 StartCoroutine으로 상태머신 동작
		/// </summary>
		//public void StartCoroutine<STATE>()
		//	where STATE : BASE_STATE_TYPE, new()
		//{
		//	_coroutine = Runner.Instance.StartCoroutine( Coroutine<STATE>() );
		//}
		
		private void StopState( Type type )
		{
			if( null != type && _stateDic.ContainsKey( type ) )
			{
				_stateDic[type].Stop();
			}
		}

		private void SetCurrentState<STATE>()
			where STATE : BASE_STATE_TYPE, new()
		{
			_currentStateType = typeof( STATE );

			if( !_stateDic.ContainsKey( _currentStateType ) )
			{
				_stateDic.Add( _currentStateType, new STATE() );
			}
			else
			{
				if( null == _stateDic[_currentStateType] )
				{
					_stateDic[_currentStateType] = new STATE();
				}
			}

			_currentState = _stateDic[_currentStateType];
			SetOwnerStateMachine( _currentState, (MACHINE)this );
			Ready( _currentState );
		}

		virtual protected STATE ChangeState<STATE>()
			where STATE : BASE_STATE_TYPE, new()
		{
			StopState( _currentStateType );
			SetCurrentState<STATE>();
			return (STATE)_currentState;
		}
	}

	/// <summary>
	/// Owner 바로가기가 필요 없는 상태머신
	/// </summary>
	public class StateMachine : BaseStateMachine<StateMachine, State> { }

	/// <summary>
	/// Owner 바로가기 기능이 추가된 상태머신,
	///	상태머신 생성 시 대상 instance를 설정
	/// </summary>
	public class StateMachine<OWNER> : BaseStateMachine<StateMachine<OWNER>, State<OWNER>>
	{
		private OWNER _owner = default( OWNER );
		protected OWNER Owner { get { return _owner; } }

		public StateMachine( OWNER instance )
		{
			_owner = instance;
		}

		override protected STATE ChangeState<STATE>()
		{
			STATE result = base.ChangeState<STATE>();
			CurrentState.Owner = Owner;

			return result;
		}
	}
}
