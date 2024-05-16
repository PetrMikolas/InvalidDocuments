import './ErrorMessage.css';
import PropTypes from 'prop-types';

function ErrorMessage({ message }) {
    return <p className="error">{message}</p>;
}

ErrorMessage.propTypes = {
    message: PropTypes.string.isRequired,
};

export default ErrorMessage;