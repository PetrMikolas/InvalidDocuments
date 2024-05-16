import loader from './loader.gif';
import './LoadingMessage.css'; 

function LoadingMessage() {
    return (
        <div className="loading-container">
            <img className="loader" src={loader} alt="loader"/>
            <p className="loading-text">Vyhledávání...</p>
        </div>
    );
}

export default LoadingMessage;