import PropTypes from 'prop-types';

function ErrorMessage({ message }) {
    return <p style={{ color: 'red' }}>{message}</p>;
}

ErrorMessage.propTypes = {
    message: PropTypes.string.isRequired,
};

export default ErrorMessage;